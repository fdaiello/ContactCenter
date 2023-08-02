/*
 * Created By WangWei on 6/15/2020
 * This controller is for system/group administrators.
 */
using Azure.Storage.Blobs;
using System;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ContactCenter.Infrastructure.Clients.Wassenger;
using ContactCenter.Infrastructure.Clients.MayTapi;
using ContactCenter.Infrastructure.Utilities;
using ContactCenter.Helpers;
using ContactCenter.Core.Models;
using ContactCenter.Models.view;
using ContactCenter.Data;
using ContactCenter.Data.Interfaces;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using EnumsNET;

namespace ContactCenter.Controllers
{

    [Authorize]
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;       //Entity Instance for ChattingLog and Customer table
        private readonly UserManager<ApplicationUser> _userManager;        //Admin and Agent Table Managenment
        private readonly IWebHostEnvironment _environment;
        private readonly IAccountManager _accountManager;                  //Login Manager
        private readonly IConfiguration _configuration;
        private readonly Notify _notifyApi;
        private readonly CommonCalls _common;
        private readonly WassengerClient _wassengerClient;
        private readonly MayTapiClient _mayTapiClient;
        private readonly IMemoryCache _cache;

        public AdminController(IAccountManager accountManager, IWebHostEnvironment iWebHostEnvironment, ApplicationDbContext ApplicationDbContext, UserManager<ApplicationUser> userManager, Notify notifyapi, IConfiguration configuration, BlobContainerClient blobContainerClient, WassengerClient wassengerClient, MayTapiClient mayTapiClient, IMemoryCache cache)
        {
            _accountManager = accountManager;
            _environment = iWebHostEnvironment;
            _userManager = userManager;
            _applicationDbContext = ApplicationDbContext;
            _notifyApi = notifyapi;
            _configuration = configuration;
            _common = new CommonCalls(configuration, ApplicationDbContext, userManager, iWebHostEnvironment, blobContainerClient);
            _wassengerClient = wassengerClient;
            _mayTapiClient = mayTapiClient;
            _cache = cache;
        }

        #region MainMethods
        /*
         * GetAdminType return what current logined user is system administrator or group administrator.
         * If system administrator loged in , it return 2
         * Else if group administrator , return 1
         * Else if agent, return 0
         */
        private async Task<int> GetUserRoleNumber()
        {   // Get the roles for the current user
            var rs = await _userManager.GetRolesAsync(_userManager.GetUserAsync(User).Result).ConfigureAwait(false);
            foreach (string r in rs)
            {
                if (r.ToLower().Equals(new string("sysadmin")))
                {
                    return 1;
                }
                else if (r.ToLower().Equals("groupadmin".ToString()))
                {
                    return 2;
                }
                else if (r.ToLower().Equals("supervisor".ToString()))
                {
                    return 3;
                }
                else if (r.StartsWith("agent", StringComparison.OrdinalIgnoreCase))
                {
                    return 4;
                }
                else if (r.Equals("atendente", StringComparison.OrdinalIgnoreCase))
                {
                    return 5;
                }
            }
            return 0;
        }
        /*
         * LoadViewVariablesAsync generate variables needed in all view pages.
         ***params
         * page: view page name
         */
        private async Task LoadViewVariablesAsync(string page)
        {
            /*This points what current logined user is system administrator or group administrator.
            *system admin=2, group admin=1, common agent=0(maybe, there is no a case admintype=0 
            * because administrators can access into this controller)
            */
            string userRole = GetUserRoleNumber().Result.ToString();
            ViewData["admintype"] = userRole;

            //This points what current page is one, so view can activates a current page in left sidebar.
            ViewData["page"] = page;
            int groupId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
            ViewData["current_group"] = groupId;
            int departmentId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).DepartmentId ?? 0;
            ViewData["current_did"] = departmentId;

            string currentUserId = _userManager.GetUserId(User);

            //This send current logined user model object to view page.
            ViewBag.current_agent = await _common.GetCurrentUser(_userManager.GetUserId(User));

            //This data list is used group-drop/down list-view of User-Management page's add/update Dialog.
            IQueryable<GroupView> Query = from a in _applicationDbContext.Set<Group>().Select(u => new { Id = u.Id, Name = u.Name }).OrderByDescending(u => u.Id)
                                          select new GroupView { Id = a.Id, Name = a.Name, UserCount = 0, Action = "" };
            if (userRole!="1")
                Query = Query.Where(u => u.Id == groupId);

            ViewBag.GroupList = Query.ToList();

            // Department View
            IQueryable<DepartmentView> dQuery = from a in _applicationDbContext.Set<Department>().Select(u => new { Id = u.Id, Name = u.Name, GroupId = u.GroupId }).OrderByDescending(u => u.Id)
                                                select new DepartmentView { Id = a.Id, GroupId = a.GroupId, Name = a.Name, UserCount = 0, Action = "" };
            if (userRole != "1") 
                dQuery = dQuery.Where(u => u.GroupId == groupId);



            var json = JsonConvert.SerializeObject(dQuery.ToList());
            List<DepartmentView> deptList = JsonConvert.DeserializeObject<List<DepartmentView>>(json);
            ViewBag.DepartmentList = deptList;
            ViewBag.DepartmentListJson = json;

            //This data list is used group-drop/down list-view of User-Management page's add/update Dialog.
            IQueryable<RoleView> Query1 = from a in _applicationDbContext.Roles.OrderBy(u => u.CreatedDate)
                                          select new RoleView { Id = a.Id, Name = a.Name, Description = a.Description, CreatedBy = a.CreatedBy, CreatedDate = a.CreatedDate, UpdatedDate = a.UpdatedDate, UserCount = 0, Action = "" };

            // Somente o sysadmin pode criar outro sysadmin
            if (userRole != "1")
                Query1 = Query1.Where(u => !u.Name.Equals("sysadmin"));

            ViewBag.RoleList = Query1.ToList();

            ViewBag.ChannelTypeList = ChatChannelTypes;
            ViewBag.ChannelSubTypeList = ChatChannelSubTypes;

            ViewBag.FilterUsers = "";
            try
            {
                IQueryable<UserView> Query2 = from a in _applicationDbContext.Set<ApplicationUser>().OrderBy(u => u.GroupId)
                                              join b in _applicationDbContext.Set<Group>()
                                              on a.GroupId equals b.Id into g
                                              from b in g.DefaultIfEmpty()
                                              select new UserView
                                              {
                                                  Id = a.Id,
                                                  UserName = a.UserName,
                                                  FullName = userRole == "1" ? $"{b.Name} / {a.FullName}" : a.FullName,
                                                  NickName = a.NickName,
                                                  GroupId = a.GroupId,
                                                  RoleId = a.Roles.First().RoleId
                                              };
                if (userRole != "1")
                    dQuery = dQuery.Where(u => u.GroupId == groupId);

                ViewBag.FilterUsers = Query2.ToList();
            }

            catch (Exception) { }

            // Check if current user has any Board and save this info to a ViewData
            ViewData["hasAnyBoard"] = await _applicationDbContext.Boards
                         .Where(p => p.GroupId == groupId & ((p.ApplicationUserId == null & (p.DepartmentId == departmentId | p.DepartmentId == null)) | p.ApplicationUserId == currentUserId))
                         .AnyAsync();

        }


        public List<SelectListItem> ChatChannelTypes =>
         ((ChannelType[])Enum.GetValues(typeof(ChannelType)))
           .Select(item => new SelectListItem
           {
               Value = ((int)item).ToString(),
               Text = item.ToString()
           }).ToList();
        public List<SelectListItem> ChatChannelSubTypes =>
        ((ChannelSubType[])Enum.GetValues(typeof(ChannelSubType)))
          .Select(item => new SelectListItem
          {
              Value = ((int)item).ToString(),
              Text = item.ToString()
          }).ToList();
        #endregion

        #region ViewActions
        /*
         * ---------------------------------------------------------------------------------
         * These are view action methods for daashboard, group, role, user management pages.
         * ---------------------------------------------------------------------------------
         */
        public async Task<IActionResult> Index(string fdate = "", string tdate = "")
        {
            await LoadViewVariablesAsync("index").ConfigureAwait(false);
            return View();
        }

        public async Task<IActionResult> Dashboard(string fdate = "", string tdate = "")
        {
            // Name of Chache entry - concatenated with date period
            string cacheName = $"DashboardIndex-{fdate}to{tdate}";

            // Use memory Cache to return Dashboard
            var cacheEntry = _cache.GetOrCreate(cacheName, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                entry.SetPriority(CacheItemPriority.High);

                return await GenerateDashboard(fdate, tdate);
            });

            return await cacheEntry;

        }

        /*
         * Load variables and return Dashboard View
         */
        public async Task<IActionResult> GenerateDashboard(string fdate = "", string tdate = "")
        {
            await LoadViewVariablesAsync("dashboard").ConfigureAwait(false);
            ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.StartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (tdate == null || string.IsNullOrEmpty(tdate)) tdate = ViewBag.EndDate;
            if (fdate == null || string.IsNullOrEmpty(fdate)) fdate = ViewBag.StartDate;
            DateTime dateUpper = DateTime.ParseExact(tdate + " 23:59:59", "yyyy-MM-dd HH:mm:ss", new CultureInfo("en-US"));
            DateTime dateLower = DateTime.ParseExact(fdate + " 00:00:00", "yyyy-MM-dd HH:mm:ss", new CultureInfo("en-US"));
            ViewData["FilterFromDate"] = fdate;
            ViewData["FilterToDate"] = tdate;

             List<CountMessageView> list = new List<CountMessageView>();

            //Users by day
            ViewBag.UsersByDay = "";
            list = new List<CountMessageView>();
            int maxv = 0;
            int minv = -1;

            try
            {
                IQueryable<CountMessageView> Query = _applicationDbContext.Set<ChattingLog>()
                    .Where(u =>
                        u.Time >= dateLower && u.Time <= dateUpper
                        && (ViewData["admintype"].Equals("2") || (ViewData["admintype"].Equals("1") && int.Parse(ViewData["current_group"].ToString()) == u.GroupId))
                    )
                    .OrderBy(u => u.Time)
                    .GroupBy(u => new
                    {
                        User = u.ContactId,
                        Date = u.Time.Year.ToString() + (u.Time.Month < 10 ? "-0" : "-") + u.Time.Month.ToString() + (u.Time.Day < 10 ? "-0" : "-") + u.Time.Day.ToString()
                    })
                    .Select(u => new CountMessageView
                    { //
                        MessageDate = DateTime.ParseExact(u.Key.Date.ToString(), "yyyy-MM-dd", CultureInfo.CurrentUICulture).ToString("dd MMM"),
                        ContactId = u.Key.User,
                        MessageCount = u.Count()
                    });//

                List<CountMessageView> tlist = Query.ToList();
                string prev_date = "";
                int cnt = 0;
                foreach (CountMessageView mv in tlist)
                {
                    if (prev_date.Equals("")) prev_date = mv.MessageDate;
                    if (!prev_date.Equals(mv.MessageDate))
                    {
                        list.Add(new CountMessageView
                        {
                            MessageDate = prev_date,
                            MessageCount = cnt
                        });
                        prev_date = mv.MessageDate;
                        cnt = 0;
                    }
                    cnt++;
                }
                list.Add(new CountMessageView
                {
                    MessageDate = prev_date,
                    MessageCount = cnt
                });
                foreach (CountMessageView mv in list)
                {
                    if (maxv < mv.MessageCount) maxv = mv.MessageCount;
                    if (minv < 0) minv = mv.MessageCount;
                    else if (minv > mv.MessageCount) minv = mv.MessageCount;
                }
            }
            catch (Exception) { }
            ViewBag.UsersByDay = JsonConvert.SerializeObject(Json(
                new
                {
                    min = minv,
                    max = maxv,
                    count = list.Count,
                    list = list
                }
            ));

            //Messages By Day
            ViewBag.MessagesByDay = "";
            ViewBag.TotalMessagesCount = 0;
            maxv = 0; minv = -1;
            list = new List<CountMessageView>();
            try
            {
                IQueryable<CountMessageView> Query = _applicationDbContext.Set<ChattingLog>()
                    .Where(u =>
                        u.Time >= dateLower && u.Time <= dateUpper
                        && (ViewData["admintype"].Equals("2") || (ViewData["admintype"].Equals("1") && int.Parse(ViewData["current_group"].ToString()) == u.GroupId))
                        && u.ContactId != null
                    )
                    .OrderBy(u => u.Time)
                    .GroupBy(u => u.Time.Year.ToString() + (u.Time.Month < 10 ? "-0" : "-") + u.Time.Month.ToString() + (u.Time.Day < 10 ? "-0" : "-") + u.Time.Day.ToString())
                    .Select(u => new CountMessageView
                    { //
                        MessageDate = DateTime.ParseExact(u.Key.ToString(), "yyyy-MM-dd", CultureInfo.CurrentUICulture).ToString("dd MMM"),
                        MessageCount = u.Count()
                    });
                list = Query.ToList();
                foreach (CountMessageView mv in list)
                {
                    if (maxv < mv.MessageCount) maxv = mv.MessageCount;
                    if (minv < 0) minv = mv.MessageCount;
                    else if (minv > mv.MessageCount) minv = mv.MessageCount;
                    ViewBag.TotalMessagesCount += mv.MessageCount;
                }
            }
            catch (Exception) { }
            ViewBag.MessagesByDay = JsonConvert.SerializeObject(Json(
                new
                {
                    min = minv,
                    max = maxv,
                    count = list.Count,
                    list = list
                }
            ));


            //Channel By Day
            ViewBag.ChannelByDay = "";
            maxv = 0; minv = -1;
            list = new List<CountMessageView>();
            try
            {
                IQueryable<CountMessageView> Query = from a in (_applicationDbContext.Set<ChattingLog>()
                                                .Where(u =>
                                                    u.Time >= dateLower && u.Time <= dateUpper
                                                    && (ViewData["admintype"].Equals("2") || (ViewData["admintype"].Equals("1") && int.Parse(ViewData["current_group"].ToString()) == u.GroupId))
                                                    && u.ContactId != null
                                                )
                                             )
                                                join b in _applicationDbContext.Set<Contact>()
                                                on a.ContactId equals b.Id into g
                                                from b in g.DefaultIfEmpty()
                                                group a by b.ChannelType into h
                                                select new CountMessageView
                                                {
                                                    MessageDate = h.Key.ToString(),
                                                    MessageCount = h.Count()
                                                };
                List<CountMessageView> tlist = Query.ToList();
                foreach (CountMessageView mv in tlist)
                {
                    int f = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (mv.MessageDate.Equals(list[i].MessageDate))
                        {
                            list[i].MessageCount += mv.MessageCount;
                            f = 1;
                            break;
                        }
                    }
                    if (f == 0)
                    {
                        list.Add(mv);
                    }
                }
            }
            catch (Exception) { }
            ViewBag.ChannelByDay = JsonConvert.SerializeObject(Json(
                new
                {
                    count = list.Count,
                    list
                }
            ));

            ViewBag.ChannelTypeList = ChatChannelTypes;
            ViewBag.ChannelSubTypeList = ChatChannelSubTypes;
            ApplicationUser usr = await _applicationDbContext.Users.FindAsync(_userManager.GetUserId(User)).ConfigureAwait(false);
            ViewBag.ApplicationUserList = usr;

            return View();
        }
        public async Task<IActionResult> GroupView()
        {
            await LoadViewVariablesAsync("group");
            return View();
        }
        public async Task<IActionResult> DepartmentView(int gid = 0)
        {
            await LoadViewVariablesAsync("department");
            if (gid == 0) gid = int.Parse(ViewData["current_group"].ToString());
            ViewBag.gid = gid;
            return View();
        }
        public async Task<IActionResult> RoleView()
        {
            await LoadViewVariablesAsync("role");
            return View();
        }
        public async Task<IActionResult> UserView()
        {
            await LoadViewVariablesAsync("user");
            return View();
        }

        public async Task<IActionResult> ChatChannelView()
        {
            await LoadViewVariablesAsync("chatchannel");

            return View();
        }

        public async Task<IActionResult> FieldsView() 
        {
            await LoadViewVariablesAsync("fields");

            return View();
        }
        public async Task<IActionResult> BoardView()
        {
            await LoadViewVariablesAsync("board");

            return View();
        }
        public async Task<IActionResult> BoardFieldsView()
        {
            await LoadViewVariablesAsync("boardfields");

            return View();
        }
        public async Task<IActionResult> BoardStagesView()
        {
            await LoadViewVariablesAsync("boardstages");

            return View();
        }

        public async Task<IActionResult> ContactFieldsView()
        {
            await LoadViewVariablesAsync("contactfields");

            return View();
        }
        public async Task<IActionResult> WeekScheduleView()
        {
            await LoadViewVariablesAsync("weekschedule");

            int groupId = int.Parse(ViewData["current_group"].ToString());

            IQueryable<WeekSchedule> Query = _applicationDbContext.WeekSchedules
                .Where(u => u.GroupId == groupId);

            WeekSchedule schedule;
            if (Query.Count() == 0)
            {
                schedule = new WeekSchedule
                {
                    DepartmentId = null,
                    GroupId = int.Parse(ViewData["current_group"].ToString()),
                    SunOpen = "",
                    SunClose = "",
                    MonOpen = "",
                    MonClose = "",
                    TueOpen = "",
                    TueClose = "",
                    WedOpen = "",
                    WedClose = "",
                    ThuOpen = "",
                    ThuClose = "",
                    FriOpen = "",
                    FriClose = "",
                    SatOpen = "",
                    SatClose = ""
                };
                await _applicationDbContext.WeekSchedules.AddAsync(schedule).ConfigureAwait(false);
                await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                schedule = Query.FirstOrDefault();
            }
            ViewBag.Schedule = JsonConvert.SerializeObject(schedule);
            ViewBag.OutOfOfficeHoursPhrase = schedule.OutOfOfficeHoursPhrase;

            return View();
        }
        /*
         * Save WeekSchedule
         */
        [HttpPost]
        public async Task<JsonResult> SaveWeekSchedule(string week, string opentime, string closetime, string outoffofficephrase)
        {
            WeekSchedule sch = null;
            if (opentime == null) opentime = "";
            if (closetime == null) closetime = "";
            try
            {
                ApplicationUser user = await _applicationDbContext.Users.FindAsync(_userManager.GetUserId(User)).ConfigureAwait(false);
                int groupId = user.GroupId;

                if (!string.IsNullOrEmpty(week))
				{
                    sch = _applicationDbContext.WeekSchedules
                        .Where(u => u.GroupId == groupId)
                        .Where(u => u.DepartmentId == null).FirstOrDefault();
                    if (week.Equals("sun")) { sch.SunOpen = opentime; sch.SunClose = closetime; }
                    else if (week.Equals("mon")) { sch.MonOpen = opentime; sch.MonClose = closetime; }
                    else if (week.Equals("tue")) { sch.TueOpen = opentime; sch.TueClose = closetime; }
                    else if (week.Equals("wed")) { sch.WedOpen = opentime; sch.WedClose = closetime; }
                    else if (week.Equals("thu")) { sch.ThuOpen = opentime; sch.ThuClose = closetime; }
                    else if (week.Equals("fri")) { sch.FriOpen = opentime; sch.FriClose = closetime; }
                    else if (week.Equals("sat")) { sch.SatOpen = opentime; sch.SatClose = closetime; }

                    sch.OutOfOfficeHoursPhrase = outoffofficephrase;

                    _applicationDbContext.Entry(sch).State = EntityState.Modified;

                    // Salva a frase também em Bot Settings
                    var botSettings = await _applicationDbContext.BotSettings
                                    .Where(p => p.GroupId == groupId)
                                    .FirstOrDefaultAsync();
                    if (botSettings != null)
					{
                        botSettings.OutOfOfficeHoursPhrase = outoffofficephrase;
                        _applicationDbContext.BotSettings.Update(botSettings);
					}

                    _applicationDbContext.SaveChanges();
                }

            }
            catch (Exception) { }
            return Json(new
            {
                schedule = JsonConvert.SerializeObject(sch)
            });
        }

        public async Task<IActionResult> TemplatesView()
        {
            await LoadViewVariablesAsync("templates").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> ImportsView()
        {
            await LoadViewVariablesAsync("imports").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> MessagesView()
        {
            await LoadViewVariablesAsync("messages").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> LandingsView()
        {
            await LoadViewVariablesAsync("landings").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> SendingsView()
        {
            await LoadViewVariablesAsync("sendings").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> GroupCampaignsView()
        {
            await LoadViewVariablesAsync("groupcampaigns").ConfigureAwait(false);
            return View();
        }
        public async Task<IActionResult> AutomationsView()
        {
            await LoadViewVariablesAsync("automations").ConfigureAwait(false);
            return View();
        }
        public IActionResult EditMailMessage()
        {
            return View();
        }
        public IActionResult EditLanding()
        {
            return View();
        }        /*
         * ---------------------------------------------------------------------------------
         *                         End of view action methods
         * ---------------------------------------------------------------------------------
         */

        #endregion

        #region GroupApi
        /*
         * GetGroupList
         */
        [HttpPost]
        public async Task<JsonResult> GetGroupListAsync()
        {
            List<GroupView> groupList = new List<GroupView>();

            IQueryable<GroupView> Query = from a in _applicationDbContext.Set<Group>().Select(u => new { Id = u.Id, Name = u.Name, Descr = u.Descr, CreatedDate = u.CreatedDate, BotUrl = u.BotUrl }).OrderByDescending(u => u.Id)
                                              select new GroupView { Id = a.Id, Name = a.Name, UserCount = 0, Action = "", Descr = a.Descr, CreatedDate = a.CreatedDate, BotUrl=a.BotUrl };
            groupList = await Query.ToListAsync();
            for (int i = 0; i < groupList.Count; i++)
            {
                groupList[i].CreatedAt = groupList[i].CreatedDate.Day + "/" + groupList[i].CreatedDate.Month + "/" + groupList[i].CreatedDate.Year;
                groupList[i].UserCount = _applicationDbContext.Set<ApplicationUser>().Where(u => u.GroupId == groupList[i].Id).Count();
                groupList[i].Action = "<div class=\"tools\">";
                if (groupList[i].UserCount == 0)
                    groupList[i].Action += $"<a onclick=\"deleteGroup({groupList[i].Id});\" title=\"delete\" class=\"delete-row\" aria-describedby=\"ui-tooltip-0\"><span class=\"delete-icon-custom\"></span></a>";
                groupList[i].Action += $"<a data-toggle=\"modal\" data-target=\"#modal-group-add\" onclick=\"editGroup({groupList[i].Id},'{groupList[i].Name}','{groupList[i].Descr}','{groupList[i].BotUrl}');\" title=\"edit\" class=\"edit_button\" aria-describedby=\"ui-tooltip-0\"><span class=\"edit-icon-custom\"></span></a></div>";
            }

            return Json(new
            {
                groupList
            });
        }
        /*
         * SaveGroup
         */
        [HttpPost]
        public async Task<JsonResult> SaveGroup(int id, string name, string descr, string botUrl)
        {
            if (!Utility.IsValidURI(botUrl))
            {
                return Json(new { msg = "A URL é inválida!" });
            }
            Group group = _applicationDbContext.Groups.Find(id);
            if (group == null)
            {
                await _applicationDbContext.Groups.AddAsync(new Group { Name = name, UserCount = 0, CreatedDate = DateTime.Now, Descr = descr, BotUrl = new Uri(botUrl)}).ConfigureAwait(false);
                await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                group.Name = name;
                group.Descr = descr;
                group.BotUrl = new Uri(botUrl);
                _applicationDbContext.Entry(group).State = EntityState.Modified;
                _applicationDbContext.SaveChanges();
            }

            return Json(new
            {
                msg = "ok"
            });

        }
        /*
         * DeleteGroup
         */
        [HttpPost]
        public async Task<JsonResult> DeleteGroupAsync(int id)
        {

            Group group = _applicationDbContext.Groups.Find(id);
            if (group == null)
            {
                return Json(new
                {
                    msg = $"Grupo {id} não encontrado!"
                });
            }
            else
            {
                _applicationDbContext.Groups.Remove(group);
                await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            return Json(new
            {
                msg = "ok"
            });
        }

        #endregion

        #region DepartmentApi
        //department
        /*
         * GetDepartmentList
         */
        [HttpPost]
        public JsonResult GetDepartmentList(int gid = 0)
        {
            List<DepartmentView> departmentList = new List<DepartmentView>();
            try
            {
                IQueryable<DepartmentView> Query = from a in _applicationDbContext.Set<Department>().Where(u => u.GroupId == gid).OrderByDescending(u => u.Id)
                                                   select new DepartmentView { Id = a.Id, Name = a.Name, UserCount = 0, Action = "", CreatedDate = a.CreatedDate };
                departmentList = Query.ToList();
                for (int i = 0; i < departmentList.Count; i++)
                {
                    departmentList[i].UserCount = _applicationDbContext.Set<ApplicationUser>().Where(u => u.DepartmentId == departmentList[i].Id).Count();
                    departmentList[i].CreatedAt = departmentList[i].CreatedDate.Day + "/" + departmentList[i].CreatedDate.Month + "/" + departmentList[i].CreatedDate.Year;
                    departmentList[i].Action = "<div class=\"tools\">";
                    if (departmentList[i].UserCount == 0) departmentList[i].Action += $"<button onclick=\"deleteDepartment({departmentList[i].Id});\" title=\"excluir\" class=\"btn btn-danger\" aria-describedby=\"excluir\"><i class=\"fa fa-trash\"></i></button>";
                    departmentList[i].Action += $"<button data-toggle=\"modal\" data-target=\"#modal-department-add\" onclick=\"editDepartment({departmentList[i].Id},'{departmentList[i].Name}');\" title=\"editar\" class=\"btn btn-warning\" aria-describedby=\"Editar atendente\"><i class=\"fa fa-edit\"></i></button></div>";
                }
            }
            catch (Exception) { }
            return Json(new
            {
                departmentList
            });
        }
        /*
         * SaveGroup
         */
        [HttpPost]
        public async Task<JsonResult> SaveDepartment(int id, string name, int gid)
        {
            try
            {
                Department department = _applicationDbContext.Departments.Find(id);
                if (department == null)
                {
                    await _applicationDbContext.Departments.AddAsync(new Department { GroupId = gid, Name = name, UserCount = 0, CreatedDate = DateTime.Now }).ConfigureAwait(false);
                    await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    department.GroupId = gid;
                    department.Name = name;
                    _applicationDbContext.Entry(department).State = EntityState.Modified;
                    _applicationDbContext.SaveChanges();
                }
            }
            catch (Exception) { }
            return Json(new
            {
                msg = "ok"
            });
        }
        /*
         * DeleteGroup
         */
        [HttpPost]
        public async Task<JsonResult> DeleteDepartmentAsync(int id)
        {
            try
            {
                _applicationDbContext.Departments.Remove(_applicationDbContext.Departments.Find(id));
                await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception) { }
            return Json(new
            {
                msg = "ok"
            });
        }
        #endregion

        #region RoleApi

        /*
         * GetRoleList
         */
        [HttpPost]
        public async Task<JsonResult> GetRoleListAsync()
        {
            List<RoleView> roleList = new List<RoleView>();
            try
            {
                IQueryable<RoleView> Query = from a in _applicationDbContext.Roles.OrderBy(u => u.CreatedDate)
                                             select new RoleView { Id = a.Id, Name = a.Name, Description = a.Description, CreatedBy = a.CreatedBy, CreatedDate = a.CreatedDate, UpdatedDate = a.UpdatedDate, UserCount = 0, Action = "" };
                roleList = await Query.ToListAsync().ConfigureAwait(false);
                for (int i = 0; i < roleList.Count; i++)
                {
                    roleList[i].CreatedAt = roleList[i].CreatedDate.Day + "/" + roleList[i].CreatedDate.Month + "/" + roleList[i].CreatedDate.Year;
                    roleList[i].UserCount = _userManager.GetUsersInRoleAsync(roleList[i].Name).Result.Count;
                    roleList[i].Action = "<div class=\"tools\">";
                    if (roleList[i].UserCount == 0) roleList[i].Action += $"<a onclick=\"deleteRole('{roleList[i].Id}');\" title=\"delete\" class=\"delete-row\" aria-describedby=\"ui-tooltip-0\"><span class=\"delete-icon-custom\"></span></a>";
                    roleList[i].Action += $"<a data-toggle=\"modal\" data-target=\"#modal-role-add\" onclick=\"editRole('{roleList[i].Id}','{roleList[i].Name}','{roleList[i].Description}');\" title=\"edit\" class=\"edit_button\" aria-describedby=\"ui-tooltip-0\"><span class=\"edit-icon-custom\"></span></a></div>";
                }
            }
            catch (Exception) { }
            return Json(new
            {
                roleList
            });
        }
        /*
         * SaveRole
         */
        [HttpPost]
        public async Task<JsonResult> SaveRole(string id, string name, string description)
        {
            try
            {
                ApplicationRole role = _applicationDbContext.Roles.Find(id);
                string[] claims = ApplicationPermissions.GetAllPermissionValues();
                if (role == null)
                {
                    await _accountManager.CreateRoleAsync(new ApplicationRole(name, description), claims).ConfigureAwait(false);
                }
                else
                {
                    role.Name = name;
                    role.Description = description;
                    await _accountManager.UpdateRoleAsync(role, claims).ConfigureAwait(false);
                }
            }
            catch (Exception) { }
            return Json(new
            {
                msg = "ok"
            });
        }
        /*
         * DeleteRole
         */
        [HttpPost]
        public async Task<JsonResult> DeleteRole(string id)
        {
            try
            {
                await _accountManager.DeleteRoleAsync(_applicationDbContext.Roles.Find(id)).ConfigureAwait(false);
            }
            catch (Exception) { }
            return Json(new
            {
                msg = "ok"
            });
        }
        #endregion

        #region UserApi
        /*
         * GetUserList
         */
        [HttpPost]
        public async Task<JsonResult> GetUserListAsync()
        {
            List<UserView> userList = new List<UserView>();
            try
            {
                IQueryable<UserView> Query = from a in _applicationDbContext.Users.OrderBy(u => u.GroupId).OrderBy(u => u.CreatedDate)
                                             select new UserView
                                             {
                                                 Id = a.Id,
                                                 UserName = a.UserName,
                                                 Email = a.Email,
                                                 FullName = a.FullName,
                                                 NickName = a.NickName,
                                                 Avatar = a.PictureFile,
                                                 CreatedDate = a.CreatedDate,
                                                 Action = "",
                                                 GroupId = a.GroupId,
                                                 DepartmentId = a.DepartmentId,
                                                 RoleId = a.Roles.First().RoleId,
                                                 Notification = a.Notification
                                             };
                ViewData["admintype"] = GetUserRoleNumber().Result.ToString();
                ViewData["current_group"] = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
                if (!ViewData["admintype"].Equals("1")) Query = Query.Where(u => u.GroupId == int.Parse(ViewData["current_group"].ToString()));

                userList = await Query.ToListAsync().ConfigureAwait(false);
                for (int i = 0; i < userList.Count; i++)
                {


                    userList[i].CreatedAt = userList[i].CreatedDate.Day + "/" + userList[i].CreatedDate.Month + "/" + userList[i].CreatedDate.Year;
                    userList[i].GroupName = _applicationDbContext.Groups.Find(userList[i].GroupId) == null ? "" : _applicationDbContext.Groups.Find(userList[i].GroupId).Name;
                    userList[i].RoleName = _applicationDbContext.Roles.Find(userList[i].RoleId) == null ? "" : _applicationDbContext.Roles.Find(userList[i].RoleId).Name;
                    userList[i].DepartmentName = userList[i].DepartmentId == 0 ? "" : _applicationDbContext.Departments.Find(userList[i].DepartmentId) == null ? "" : _applicationDbContext.Departments.Find(userList[i].DepartmentId).Name;

                    if (string.IsNullOrEmpty(userList[i].Avatar))
                        userList[i].Avatar = "/assets-chatroom/images/avatars/default.png";
                    else
                        userList[i].Avatar = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), userList[i].Avatar);

                    userList[i].Action = "<div class=\"tools\">";
                    userList[i].Action += $"<button onclick=\"deleteUser('{userList[i].Id}');\" title=\"Excluir\" class=\"btn btn-danger btn-delete\" aria-describedby=\"Excluir\"><i class=\"fa fa-trash\"></i></button>";
                    userList[i].Action += $"<button data-toggle=\"modal\" data-target=\"#modal-user-add\" onclick=\"editUser('{userList[i].Id}','{userList[i].UserName}','{userList[i].FullName}','{userList[i].NickName}','{userList[i].Email}','{userList[i].GroupId}','{userList[i].RoleName}','{userList[i].Avatar}','{userList[i].Notification}');\" title=\"Editar\" class=\"btn btn-warning btn-edit\" aria-describedby=\"Editar\"><i class=\"fa fa-edit\"></i></button></button></div>";
                }
            }
            catch (Exception) { }
            return Json(new
            {
                userList
            });
        }

        [HttpPost]
        public async Task<JsonResult> SaveUserAsync(
            string id,
            List<IFormFile> avatar,
            string username,
            string fullname,
            string nickname,
            string email,
            int groupId,
            int? departmentId,
            string role,
            int notification
        )
        {
            string msg = "ok";
            if (departmentId == 0)
                departmentId = null;

            try
            {

                string filename = await _common.SaveUserAvatar(avatar, id, groupId).ConfigureAwait(false);
                ApplicationUser user = _applicationDbContext.Users.Find(id);
                if (user == null)
                {
                    var createRet = await _accountManager.CreateUserAsync(
                        new ApplicationUser
                        {
                            GroupId = groupId,
                            DepartmentId = departmentId,
                            UserName = username,
                            FullName = fullname,
                            NickName = nickname,
                            Email = email,
                            PictureFile = filename,
                            Notification = (NotificationLevel)notification
                        },
                        new string[] { role }, "tempP@ss123").ConfigureAwait(false);

                    if ( !createRet.Succeeded)
					{
                        msg = createRet.Errors[0].ToString();
					}
                }
                else
                {
                    user.GroupId = groupId;
                    user.DepartmentId = departmentId;
                    user.FullName = fullname;
                    user.NickName = nickname;
                    user.Email = email;
                    user.PictureFile = filename;
                    user.Notification = (NotificationLevel)notification;

                    await _accountManager.UpdateUserAsync(user, new string[] { role }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException;
            }
            return Json(new
            {
                msg
            });
        }
        /*
         * DeleteUser
         */
        [HttpPost]
        public async Task<JsonResult> DeleteUser(string id)
        {
            string msg = "ok";

            try
            {
                var list = _applicationDbContext.ChattingLogs.Where(u => u.ApplicationUserId == id).ToList();
                foreach (ChattingLog c in list)
                {
                    _applicationDbContext.ChattingLogs.Remove(c);
                }
                _applicationDbContext.SaveChanges();
                await _accountManager.DeleteUserAsync(_applicationDbContext.Users.Find(id)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException;
            }
            return Json(new
            {
                msg
            });
        }
        /*
         * ResetUserPassword
         */
        [HttpPost]
        public async Task<JsonResult> ResetUserPassword(string id)
        {
            string msg = "ok";

            try
            {
                await _accountManager.ResetPasswordAsync(_applicationDbContext.Users.Find(id), "tempP@ss123").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException;
            }
            return Json(new
            {
                msg
            });
        }

        #endregion

        #region ChatChannel 

        [HttpPost]
        public async Task<JsonResult> GetDeviceStatus(string deviceId)
        {
            string status = string.Empty;
            string msg = "ok";

            try
            {
                // Searches Channel at Database
                ChatChannel channel = _applicationDbContext.ChatChannels.Find(deviceId);

                // If channel is of Wassenger subtype
                if (channel != null && channel.ChannelType==ChannelType.WhatsApp && channel.ChannelSubType==ChannelSubType.Alternate1)
                {
                    status = await _wassengerClient.GetAndSaveStatus(deviceId).ConfigureAwait(false);
                }
                // If channel is of MayTypi subtype
                else if (channel != null && channel.ChannelType == ChannelType.WhatsApp && channel.ChannelSubType == ChannelSubType.Alternate2)
                {
                    status = await _mayTapiClient.GetAndSaveStatus(deviceId).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException;
            }
            return Json(new
            {
                msg,
                status,
                deviceId
            });
        }

        [HttpPost]
        public async Task<JsonResult> ScanDevice(string deviceId)
        {

            string body = string.Empty;
            string content = string.Empty;
            int statuscode = (int)HttpStatusCode.OK;

            try
            {
                // Search channel at Db
                ChatChannel channel = _applicationDbContext.ChatChannels.Find(deviceId);

                // If channel is of Wassenger subtype
                if (channel != null && channel.ChannelType == ChannelType.WhatsApp && channel.ChannelSubType == ChannelSubType.Alternate1)
                {
                    content = await _wassengerClient.ScanDevice(deviceId).ConfigureAwait(false);

                }
                // If channel is of MayTapy subtype
                else if (channel != null && channel.ChannelType == ChannelType.WhatsApp && channel.ChannelSubType == ChannelSubType.Alternate2)
                {
                    content = $"<img alt=\"QrCode\" src=\"\\Admin\\MtQrCode?deviceId={deviceId}&dt={DateTime.UtcNow.ToString()}\" />";
                }
            }
            catch ( Exception ex)
            {
                content = ex.Message;
                statuscode = (int)HttpStatusCode.PreconditionFailed;
            }

            return Json(new
            {
                content = content,
                contenttype = "text/html",
                statuscode = statuscode,
            });

        }
        [HttpGet]
        public async Task<ActionResult> MtQrCode(string deviceId)
        {
            // Gets logged user groupId
            int groupId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            // Check if ChatChannel Id existe at database
            ChatChannel chatChannel = await _applicationDbContext.ChatChannels
                                .Where(p => p.Id == deviceId && p.GroupId==groupId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (chatChannel != null)

                return base.File(await _mayTapiClient.QrCode(deviceId, chatChannel.Login, chatChannel.Password).ConfigureAwait(false), "image/png");
            else
                return BadRequest($"deviceId {deviceId} not found!");
        }

        [HttpPost]
        public async Task<ActionResult> RedeployChatChannel(string id)
        {
            // Gets logged user groupId
            int groupId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            // Check if ChatChannel Id existe at database
            ChatChannel chatChannel = await _applicationDbContext.ChatChannels
                                .Where(p => p.Id == id && p.GroupId == groupId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (chatChannel != null)
			{
                var message = await _mayTapiClient.Redeploy(id, chatChannel.Login, chatChannel.Password);

                return Json(new
                {
                    message
                });

            }
            else
			{
                return BadRequest($"deviceId {id} not found!");

            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveChatChannel(ChatChannel chatChannel)
        {
            // Se informou AppService name, confere se é unico na base
            if (!string.IsNullOrEmpty(chatChannel.AppName))
            {
                // Check if ChatChannel Id existe at database
                ChatChannel oldChatChannel = await _applicationDbContext.ChatChannels
                                    .Where(p => p.AppName == chatChannel.AppName && p.Id != chatChannel.Id)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                // Se já tem algum outro canal com mesmo AppName
                if (oldChatChannel != null)
                {
                    return Json(new
                    {
                        msg = $"Já existe na base outro canal com este mesmo AppName: {chatChannel.AppName}"
                    });
                }
            }

            try
            {

                ChatChannel channel = _applicationDbContext.ChatChannels.Find(chatChannel.Id);
                chatChannel.GroupId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

                if (channel == null)
                {

                    await _applicationDbContext.ChatChannels.AddAsync(new ChatChannel
                    {
                        Id = chatChannel.Id,
                        GroupId = chatChannel.GroupId,
                        Name = chatChannel.Name,
                        PhoneNumber = chatChannel.PhoneNumber,
                        AppName = chatChannel.AppName,
                        ChannelType = chatChannel.ChannelType,
                        ChannelSubType = chatChannel.ChannelSubType,
                        ApplicationUserId = chatChannel.ApplicationUserId,
                        DepartmentId = chatChannel.DepartmentId > 0 ? chatChannel.DepartmentId :  null,
                        Host = chatChannel.Host,
                        Login = chatChannel.Login,
                        Password = chatChannel.Password,
                        Status = chatChannel.Status,
                        From = chatChannel.From
                    }).ConfigureAwait(false);
                    await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    channel.GroupId = chatChannel.GroupId;
                    channel.Name = chatChannel.Name;
                    channel.PhoneNumber = chatChannel.PhoneNumber;
                    channel.AppName = chatChannel.AppName;
                    channel.ChannelType = chatChannel.ChannelType;
                    channel.ChannelSubType = chatChannel.ChannelSubType;
                    channel.ApplicationUserId = chatChannel.ApplicationUserId;
                    channel.DepartmentId = chatChannel.DepartmentId > 0 ? chatChannel.DepartmentId : null;
                    channel.Host = chatChannel.Host;
                    channel.Login = chatChannel.Login;
                    channel.Password = chatChannel.Password;
                    channel.Status = chatChannel.Status;
                    channel.From = chatChannel.From;
                    _applicationDbContext.Entry(channel).State = EntityState.Modified;
                    _applicationDbContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    msg = ex.Message
                });
            }
            return Json(new
            {
                msg = "ok"
            });
        }
        [HttpPost]
        public async Task<JsonResult> GetChatChannelList()
        {
            int groupId = _applicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
            
            List <ChatChannelView> chatChannelList = new List<ChatChannelView>();
            try
            {
                IQueryable<ChatChannelView> Query = from a in _applicationDbContext.Set<ChatChannel>().Where(u => u.GroupId == groupId).OrderByDescending(u => u.Id)
                                                    select new ChatChannelView
                                                    {
                                                        Id = a.Id,
                                                        Name = a.Name,
                                                        PhoneNumber = a.PhoneNumber,
                                                        AppName = a.AppName,
                                                        ChannelType = a.ChannelType,
                                                        ChannelSubType = a.ChannelSubType,
                                                        DepartmentId = a.DepartmentId,
                                                        ApplicationUserId = a.ApplicationUserId,
                                                        Host = a.Host,
                                                        Login = a.Login,
                                                        Password = a.Password,
                                                        Status = a.Status,
                                                        From = a.From,
                                                        TypeDescr = Utility.Description (a.ChannelType) + ( a.ChannelSubType != ChannelSubType.None ? "/" + Utility.Description (a.ChannelSubType) : string.Empty)
                                                    };
                chatChannelList = await Query.ToListAsync();

                for (int i = 0; i < chatChannelList.Count; i++)
                {
#pragma warning disable CS4014 // Queremos executar de forma assincrona as operações abaixo, e por isto não vamos usar await
                    if (chatChannelList[i].ChannelType == ChannelType.WhatsApp && chatChannelList[i].ChannelSubType == ChannelSubType.Alternate1)
                    {
                        _wassengerClient.GetAndSaveStatus(chatChannelList[i].Id).ConfigureAwait(false);
                    }
                    else if (chatChannelList[i].ChannelType == ChannelType.WhatsApp && chatChannelList[i].ChannelSubType == ChannelSubType.Alternate2)
                    {
                        _mayTapiClient.GetAndSaveStatus(chatChannelList[i].Id);
                    }
#pragma warning restore CS4014 
                    chatChannelList[i].Icon = $"{chatChannelList[i].ChannelType}.png";
                }
            }
            catch (Exception ex)
            {

                return Json(new { ex.Message });

            }
            return Json(new
            {
                chatChannelList
            });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteChatChannel(string id)
        {
            string msg = "ok";

            try
            {
                _applicationDbContext.ChatChannels.Remove(_applicationDbContext.ChatChannels.Find(id));
                await _applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            catch (Exception ex)
            {
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                    msg = ex.InnerException.Message;
                else
                    msg = ex.Message;
            }

            return Json(new
            {
                msg
            });
        }

        [HttpPost]
        public JsonResult SynchronizeContacts(string deviceId)
        {

            string body = string.Empty;
            string content = string.Empty;
            int statuscode = (int)HttpStatusCode.OK;
            int qtdSynch = 0;

            try
            {
                // Search channel at Db
                ChatChannel channel = _applicationDbContext.ChatChannels.Find(deviceId);

                // If device is MayTapi
                if (channel != null && channel.ChannelType == ChannelType.WhatsApp && channel.ChannelSubType == ChannelSubType.Alternate2)
                {
                    // Get total from SynchronizeDContacts
                    qtdSynch = _mayTapiClient.ImportedContactsCount(deviceId);

                    // Se não sinchronizou ainda
                    if (qtdSynch == -1)
                    {
                        // Fire synchronization
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        _mayTapiClient.ImportContacts(deviceId);
#pragma warning restore CS4014 
                        content = "Importando contatos do celular ...";
                    }
                    else
                    {
                        // Return quantity
                        content = $"Contatos importados: {qtdSynch}";
                    }
                }

                else if (channel != null && channel.ChannelType == ChannelType.WhatsApp && channel.ChannelSubType == ChannelSubType.Alternate1)
                {
                    // Get total from SynchronizeDContacts
                    qtdSynch = _wassengerClient.Synchronized(deviceId);

                    // Se não sinchronizou ainda
                    if (qtdSynch == -1)
                    {
                        // Fire synchronization
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        _wassengerClient.Synchronize(deviceId);
#pragma warning restore CS4014 
                        content = "Sincronizando ...";
                    }
                    else
                    {
                        // Return quantity
                        content = $"Contatos sincronizados: {qtdSynch}";
                    }
                }

                else

                    content = "Este dispositivo não suporta sincronização dos contatos.";


            }
            catch (Exception ex)
            {
                content = ex.Message;
                statuscode = (int)HttpStatusCode.PreconditionFailed;
            }

            return Json(new
            {
                content = content,
                contenttype = "text/html",
                statuscode = statuscode,
            });

        }
        #endregion

    }
}