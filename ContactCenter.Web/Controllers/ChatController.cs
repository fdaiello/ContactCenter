using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ImageMagick;
using NAudio.Lame;
using NAudio.Wave;
using ContactCenter.Infrastructure.Clients.Wpush;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Clients.Speech;
using ContactCenter.Infrastructure.Utilities;
using Azure.Storage.Blobs.Models;

namespace ContactCenter.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _ApplicationDbContext;       //DbContext
        private readonly UserManager<ApplicationUser> _userManager;        //Admin and Agent Table Managenment
        private readonly IWebHostEnvironment _environment;
        private readonly IAccountManager _accountManager;                  //Login Manager
        private readonly IConfiguration _configuration;
        private const string avapath = "/assets-chatroom/images/avatars/default.png";

        private readonly Notify _notifyApi;
        private readonly CommonCalls _common;
        private readonly WpushClient _wpushClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly SpeechClient _speechClient;

        public ChatController(
            IAccountManager accountManager, 
            IWebHostEnvironment iWebHostEnvironment, 
            ApplicationDbContext ApplicationDbContext, 
            UserManager<ApplicationUser> userManager, 
            Notify notifyapi, 
            IConfiguration configuration,
            WpushClient wpushClient,
            BlobContainerClient blobContainerClient,
            SpeechClient speechClient)
        {
            _accountManager = accountManager;
            _environment = iWebHostEnvironment;
            _userManager = userManager;
            _ApplicationDbContext = ApplicationDbContext;
            _notifyApi = notifyapi;
            _configuration = configuration;
            _common = new CommonCalls(configuration, ApplicationDbContext, userManager, iWebHostEnvironment, blobContainerClient);
            _wpushClient = wpushClient;
            _blobContainerClient = blobContainerClient;
            _speechClient = speechClient;
        }
        //[SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
        /*
         * GetAdminType return what current logined user is system administrator or group administrator.
         * If system administrator loged in , it return 2
         * Else if group administrator , return 1
         * Else if agent, return 0
         */
        private async Task<int> GetLoggedUserRoleNumber()
        {   // Get the roles for the current user
            var rs = await _userManager.GetRolesAsync(_userManager.GetUserAsync(User).Result).ConfigureAwait(false);
            foreach (string r in rs)
            {
                if (r.Equals("sysadmin",StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
                else if (r.Equals("groupadmin", StringComparison.OrdinalIgnoreCase))
                {
                    return 2;
                }
                else if (r.Equals("supervisor", StringComparison.OrdinalIgnoreCase))
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
        public async Task<IActionResult> Index()
		{
            // Busca a função do usuário logado, e salva em variavel local, e em ViewDat
            int userRoleNumber = await GetLoggedUserRoleNumber();
            bool isadmin = userRoleNumber == 1 | userRoleNumber == 2;
            ViewData["isadmin"] = isadmin;

            // Busca o ID do grupo do usuário logado
            int groupID = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
            ViewData["current_group"] = groupID;


            ViewBag.current_agent = null;

			IQueryable<ApplicationUser> query = _userManager.Users.Where(u => u.Id == _userManager.GetUserId(User));
			var user = await query.FirstOrDefaultAsync().ConfigureAwait(false);
			if (query.Any())
			{
				AgentView agent = new AgentView();
				agent.Id = user.Id;

                if (!string.IsNullOrEmpty(user.PictureFile))
                    agent.Avatar = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), user.PictureFile);
                else
                    agent.Avatar = "NULL";

                agent.Name = user.UserName ?? string.Empty;
                agent.FullName = user.FullName;
				agent.NickName = user.NickName ?? agent.Name;
				agent.Email = user.Email;
				agent.Phone = user.PhoneNumber;
                agent.Roles = _userManager.GetRolesAsync(user).Result.First();
                agent.LastMessage = string.IsNullOrEmpty(user.LastText) ? "&nbsp;": user.LastText;
                agent.LastTime = user.LastActivity.ToString(new CultureInfo ("pt-BR"));

				ViewBag.current_agent = agent;
			}

            // Insert current Roles to ViewBag
            List<ApplicationRole> roles =await _ApplicationDbContext.ApplicationRoles.OrderByDescending( p => p.Name).ToListAsync().ConfigureAwait(false);
            ViewBag.roles = roles;

            // Check if current user has any Board and save this info to a ViewData 
            ViewData["hasAnyBoard"] = await _ApplicationDbContext.Boards
                         .Where(p => p.GroupId == groupID & ( (p.ApplicationUserId == null  & ( p.DepartmentId == user.DepartmentId | p.DepartmentId == null)) | isadmin | p.ApplicationUserId == user.Id) )
                         .AnyAsync();
            
			return View();
		}

        /*
        * Traz o historico de mensagens para o chatroom
        * <SelectedId>      Id of Agent or Contact selected
        * <SelectedType>    1- SelectedID is Contact; 2- SelectedID is Agent
        * <pageSize>        number of records to return
        * <firstTime>       first ( oldest ) chatting log Time that was already retreaved; will show records greater than that
        * <messageId>       usado nas buscas, quando estamos procurando o historico de uma determinada mensagem.
        */
        [HttpPost]
		public async Task<ActionResult> GetMsgHistory(string selectedId, int selectedType, int pageSize, DateTime? firstTime, int messageId)
		{

			IQueryable<ChattingLogView> Query;

            // messageId is passed only when a message was searched and clicked at left side bar. 
            if (messageId > 0)
                // when searching - take all messages after given messageId
                pageSize = 999;

			// selecionado um Contact
			if (selectedType == 1)
				Query = (from a in _ApplicationDbContext.Set<ChattingLog>()
                         .Where(u => u.ContactId == selectedId && (firstTime == null | u.Time < firstTime) && u.Id >= messageId && (u.Source != MessageSource.A2A))
                         .OrderByDescending(u => u.Time)
						 join b in _ApplicationDbContext.Set<ApplicationUser>() on a.ApplicationUserId equals b.Id into g from b in g.DefaultIfEmpty()
						 join c in _ApplicationDbContext.Set<Contact>() on a.ContactId equals c.Id into f from c in f.DefaultIfEmpty()
                         join e in _ApplicationDbContext.Set<ChattingLog>() on a.QuotedActivityId equals e.ActivityId into es
                         from e in es.DefaultIfEmpty()
                         select new ChattingLogView { 
                             Id = a.Id,
                             ChattingLog = a, 
                             ContactName = a.ContactId == null ? "" : c.Name, 
                             AgentName = b.FullName??"celular", 
                             ToAgentName="",
                             QuotedLog = e
                         }).Take(pageSize);

			// selecionado um Agent
			else
				Query = (from a in _ApplicationDbContext.ChattingLogs
                         .Where ( u=> (u.ApplicationUserId == selectedId & u.ToAgentId == _userManager.GetUserId(User))  | (u.ApplicationUserId == _userManager.GetUserId(User) & u.ToAgentId == selectedId))
                         .Where ( u => firstTime == null | u.Time < firstTime)
                         .OrderByDescending(u => u.Time)
						 join b in _ApplicationDbContext.Set<Contact>() on a.ContactId equals b.Id into bs from b in bs.DefaultIfEmpty()
                         join c in _ApplicationDbContext.Set<ApplicationUser>() on a.ApplicationUserId equals c.Id into cs from c in cs.DefaultIfEmpty()
                         join d in _ApplicationDbContext.Set<ApplicationUser>() on a.ToAgentId equals d.Id into ds from d in ds.DefaultIfEmpty()
                         join e in _ApplicationDbContext.Set<ChattingLog>() on a.QuotedActivityId equals e.ActivityId into es
                         from e in es.DefaultIfEmpty()
                         select new ChattingLogView {
                             Id = a.Id,
                             ChattingLog = a, 
                             ContactName = a.ContactId==null ? "" : b.Name, 
                             AgentName = a.ApplicationUserId == null || string.IsNullOrEmpty(a.ApplicationUserId) ? "" : c.FullName, 
                             ToAgentName= a.ToAgentId==null|| string.IsNullOrEmpty(a.ToAgentId) ? string.Empty : d.FullName,
                             QuotedLog = e
                         }).Take(pageSize);


			var chattingLogViewList = await Query
                                    .OrderBy(u => u.ChattingLog.Time)
                                    .ToListAsync()
                                    .ConfigureAwait(false);

            // Acrescenta a URL do FileContainer ao nome dos arquivos 
            var FileContainerUrl = _configuration.GetValue<string>($"FileContainerUrl");
            foreach (ChattingLogView chat in chattingLogViewList.Where(p => !string.IsNullOrEmpty(p.ChattingLog.Filename) && !p.ChattingLog.Filename.StartsWith("http") ))
                    chat.ChattingLog.Filename = Utility.CombineUrlsToString(FileContainerUrl,chat.ChattingLog.Filename);

            // e tambem do arquivo "quotted" se tiver
            foreach (ChattingLogView chat in chattingLogViewList.Where(p => p.QuotedLog!= null && !string.IsNullOrEmpty(p.QuotedLog.Filename) && !p.QuotedLog.Filename.StartsWith("http")))
                chat.QuotedLog.Filename = Utility.CombineUrlsToString(FileContainerUrl, chat.QuotedLog.Filename);


            // Formata quebra de linha, e negrito entre asteriscos
            string[] aLinePieces;
            foreach (ChattingLogView chat in chattingLogViewList)
                if (chat.ChattingLog.Text != null)
                {
                    chat.ChattingLog.Text = chat.ChattingLog.Text.Replace("\n", "<br>", StringComparison.OrdinalIgnoreCase);
                    aLinePieces = chat.ChattingLog.Text.Split("*");
                    if (aLinePieces.Length == 3)
                        chat.ChattingLog.Text = aLinePieces[0] + "<b>" + aLinePieces[1] + "</b>" + aLinePieces[2];
                    aLinePieces = chat.ChattingLog.Text.Split("**");
                    if (aLinePieces.Length == 3)
                        chat.ChattingLog.Text = aLinePieces[0] + "<b>" + aLinePieces[1] + "</b>" + aLinePieces[2];
                }
                else
                    chat.ChattingLog.Text = string.Empty;

			////Zera contador de mensagens nao respondidas do contato atual

			//if (selectedType == 1 & !string.IsNullOrEmpty(selectedId))
			//{
			//	Contact contact = await _ApplicationDbContext.Contacts.FindAsync(selectedId);
			//	if (contact != null)
			//	{
			//		contact.UnAnsweredCount = 0;
			//		_ApplicationDbContext.Contacts.Update(contact);
			//		await _ApplicationDbContext.SaveChangesAsync();
			//	}
			//}

			return Json(new
			{
				chattingLogViewList,
				count = chattingLogViewList.Count,
			});

		}
        /*
        *adding a chatting log record
        * <attachedFile> attached file object
        * <ContactId>    to target id
        * <msgText>      adding record's chatting text
        */
        [DisableRequestSizeLimit]
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessageAsync(List<IFormFile> attachedFile, string contactId, string msgText, int isToAgent)
        {
            ChatMsgType chatMsgType = ChatMsgType.Text;
            string filename = "";
            string agentNickName = string.Empty;
            string activityId = string.Empty;
            string chattingLogId = string.Empty;
            DateTime time = Utility.HoraLocal();
            CultureInfo cultureInfo = new CultureInfo("en-US");
            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
            string msgTextRecognized = string.Empty;
            string contentType = string.Empty;

            // Remove quebra de linha final
            if (msgText != null && msgText.EndsWith("\n", StringComparison.OrdinalIgnoreCase))
                msgText = msgText.Substring(0, msgText.Length - 1);

            // Remove HTML de emojis
            msgText = Utility.CleanEmojiHTML(msgText);

            //Clean HTML from text
            msgText = Utility.CleanHTML(msgText);

            try
            {
                // Se tem arquivo anexado, salva no filesystem
                if (attachedFile != null && attachedFile.Count > 0)
                {
                    long size = attachedFile.Sum(f => f.Length);
                    string filePath = Path.Combine(_environment.WebRootPath, "attach");
                    if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
                    filename = attachedFile[0].FileName.ToLower(cultureInfo);
                    chatMsgType = Utility.GetChatMsgType(filename);

                    // Copia o arquivo para um stream de memoria
                    using (Stream memoryStream = new MemoryStream())
                    {
                        await attachedFile[0].CopyToAsync(memoryStream).ConfigureAwait(false);

                        // Se for JPG
                        if (filename.EndsWith("jpg", StringComparison.InvariantCulture) | filename.EndsWith("jpeg", StringComparison.InvariantCulture))
                        {
                            memoryStream.Position = 0;
                            using (var image = new MagickImage(memoryStream))
                            {
                                if (image.BaseWidth > 2048)
                                {
                                    image.Resize(2048, 0);
                                    image.Write(memoryStream);
                                }
                            }

                            // Seta o Content Type para o upload no Blog
                            contentType = "image/jpeg";
                        }

                        // Se for Wav
                        else if (filename.EndsWith("wav", StringComparison.InvariantCulture))
                        {
                            //// Tenta reconhecer o Audio
                            //memoryStream.Position = 0;
                            //string speechtotext = await _speechClient.RecognizeWavStream(memoryStream, DateTime.Now.ToString("yyyyMMddHHmmssfff")).ConfigureAwait(false);

                            //// Se Reconheceu com sucesso
                            //if (!string.IsNullOrEmpty(speechtotext) && speechtotext != "NOMATCH" && !speechtotext.StartsWith("CANCELED", StringComparison.OrdinalIgnoreCase))
                            //    // guarda o texto obtido na mensagem
                            //    msgTextRecognized = speechtotext;

                            // converte para MP3
                            filename = filename.Replace(".wav", ".mp3");
                            string savetofilename = filePath + $@"\{Guid.NewGuid().ToString()}-{filename}";
                            memoryStream.Position = 0;
                            using (var rdr = new WaveFileReader(memoryStream))
                            using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.ABR_128))
                            {
                                rdr.CopyTo(wtr);
                            }
                            // resseta a stream
                            memoryStream.SetLength(0);

                            // le o arquivo mp3 salvo de volta para a memória
                            using (FileStream file = new FileStream(savetofilename, FileMode.Open, FileAccess.Read))
                                file.CopyTo(memoryStream);

                            // deleta o arquivo local - não precisamos mais dele
                            System.IO.File.Delete(savetofilename);

                            // Seta o Content Type para o upload no Blog
                            contentType = "audio/mpeg";

                        }
                        // Se for outras extenções de arquivo
                        else
                        {
                            contentType = Utility.GetContentType(filename);
                        }


                        // Gera nome unico
                        filename = Utility.UniqueFileName(filename, groupId);

                        // Copia o stream para o Blob Storage
                        if (_blobContainerClient != null)
                        {
                            memoryStream.Position = 0;
                            var blobHttpHeader = new BlobHttpHeaders();
                            blobHttpHeader.ContentType = contentType;
                            BlobClient blob = _blobContainerClient.GetBlobClient(filename);
                            await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                            await blob.UploadAsync(memoryStream, blobHttpHeader).ConfigureAwait(false);
                        }

                    }
                }

                // Id do usuario ( Agente ) logado
                string currentUserId = _userManager.GetUserId(User);

                // Cria um objeto com os dados da mesagem enviada
                var chatlog = new ChattingLog
                {
                    ApplicationUserId = currentUserId,
                    Time = time,
                    Text = string.IsNullOrEmpty(msgTextRecognized) ? string.IsNullOrEmpty(msgText) ? string.Empty : msgText : msgTextRecognized,
                    Filename = filename,
                    ContactId = isToAgent == 0 ? contactId : null,
                    ToAgentId = isToAgent == 1 ? contactId : null,
                    Read = false,
                    Type = chatMsgType,
                    Source = isToAgent == 1 ? MessageSource.A2A : MessageSource.Agent,
                    GroupId = groupId
                };

                // Atualiza LastActivity e LastText do ApplicationUser atual
                ApplicationUser applicationUser = _ApplicationDbContext.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefault();
                if ( applicationUser != null )
                {
                    applicationUser.LastActivity = Utility.HoraLocal();
                    applicationUser.LastText = string.IsNullOrEmpty(msgTextRecognized) ? (string.IsNullOrEmpty(msgText) ? string.Empty : msgText) : $"\U0001F509{msgTextRecognized}";
                    agentNickName = applicationUser.NickName;
                    _ApplicationDbContext.ApplicationUsers.Update(applicationUser);
                }

                // Agent -> Contact
                if (isToAgent==0)
                {
                    // Busca o registro do Contact destinatário da mensagem
                    Contact contact = _ApplicationDbContext.Contacts.Where(u => u.Id == contactId).FirstOrDefault();
                    if (contact != null)
                    {
                        // Se o canal do contato for sms
                        if (contact.ChannelType == ChannelType.SMS)
                        {
                            // Muda o tipo da mensagem para SMS
                            chatlog.Type = ChatMsgType.TextSms;
                            chatMsgType = ChatMsgType.TextSms;
                        }

                        // Se o usuário logado é Agente ou Atendente, e o Contact destinatário está sendo atendido por outro Agente
                        if (await GetLoggedUserRoleNumber().ConfigureAwait(false) > 3 & contact.ApplicationUserId != null & contact.ApplicationUserId != currentUserId)
                            // Não envia e marca activityId null para dar erro de envio
                            activityId = null;
                        else
                        {
                            // Envia a mensagem para o contact via notificação do Bot
                            activityId = await _notifyApi.NotifyContact(contactId, msgText, filename, agentNickName).ConfigureAwait(false);

                            // Se enviou com sucesso
                            if (!string.IsNullOrEmpty(activityId) && activityId != null && activityId != "1" && !activityId.StartsWith("Erro"))
                            {
                                // Grava o Id gerado
                                chatlog.ActivityId = activityId;

                                // Inclui ChattingLog
                                await _ApplicationDbContext.ChattingLogs.AddAsync(chatlog).ConfigureAwait(false);

                                // Atualiza o Contact
                                contact.UnAnsweredCount = 0;
                                contact.LastActivity = Utility.HoraLocal();
                                contact.LastText = string.IsNullOrEmpty(msgTextRecognized) ? (string.IsNullOrEmpty(msgText) ? string.Empty : msgText) : $"\U0001F509{msgTextRecognized}";

                                // Marca a ultima mensagem enviada por este Contact como tendo sido respondida pelo atendente 
                                ChattingLog chattingLog = _ApplicationDbContext.ChattingLogs.Where(u => u.ContactId == contactId).OrderByDescending(p => p.Time).FirstOrDefault();
                                if (chattingLog != null)
                                {
                                    chattingLog.ApplicationUserId = currentUserId;
                                    _ApplicationDbContext.ChattingLogs.Update(chattingLog);
                                }

                                // Se o agente digitou SAIR
                                if (msgText != null && msgText.ToUpperInvariant() == "SAIR")
                                {
                                    // Status volta pro Bot
                                    contact.Status = ContactStatus.TalkingToBot;
                                }
                                else
                                {
                                    // Status é falando com Atendente
                                    contact.Status = ContactStatus.TalkingToAgent;
                                }
                                _ApplicationDbContext.Update(contact);

                                // Salva alterações
                                await _ApplicationDbContext.SaveChangesAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                return Json(new
                                {
                                    chattingLogId = activityId,
                                });
                            }
                        }
                    }
                }
                // Agent -> Agent 
                else
                {
                    // Atualiza LastActivity e LastText do ApplicationUser atual
                    applicationUser = _ApplicationDbContext.ApplicationUsers.Where(u => u.Id == contactId).FirstOrDefault();
                    if (applicationUser != null)
                    {
                        applicationUser.LastActivity = Utility.HoraLocal();
                        applicationUser.LastText = string.IsNullOrEmpty(msgText) ? string.Empty : msgText;
                        _ApplicationDbContext.ApplicationUsers.Update(applicationUser);
                        if (msgText == null)
                            msgText = "Arquivo enviado!";

                        // Sends WebPush Notification for the destination Agent
                        if ( applicationUser.WebPushId != "false" & applicationUser.Notification != NotificationLevel.None)
                        {
                            await _wpushClient.SendNotification( agentNickName, msgText, _configuration.GetValue<string>($"ContactCenterUrl"), applicationUser.WebPushId)
                                              .ConfigureAwait(false);
                        }
                    }

                    // Atualiza UnReadChat do Usuario logado par o Agente para o qual a mensagem foi enviada
                    await UpdateUnreadChat(currentUserId, contactId, 1);
                    // Atualiza UnReadChat do Agente destino para o Usuario atual - somente data e hora - para listar no left side bar
                    await UpdateUnreadChat(contactId, currentUserId, 0);

                    // Salva no banco de dados
                    await _ApplicationDbContext.ChattingLogs.AddAsync(chatlog).ConfigureAwait(false);
                    await _ApplicationDbContext.SaveChangesAsync().ConfigureAwait(false);
                }

                chattingLogId = chatlog.Id.ToString(new CultureInfo("en-Us"));

            }
            catch (Exception e)
            {
                return Json(new
                {
                    chattingLogId = "Erro: " + e.Message,
                });
            }

            // Se tem arquivo
            if ( filename != string.Empty )
			{
                // Acrescenta a URL do FileContainer ao nome dos arquivos
                var FileContainerUrl = _configuration.GetValue<string>($"FileContainerUrl");
                filename = Utility.CombineUrlsToString(FileContainerUrl, filename);
            }

            return Json(new
            {
                chattingLogId,
                filename,
                chatMsgType,
                time,
                msgTextRecognized
            }); 
        }

        public async Task UpdateUnreadChat(string fromApplicationUserID, string toApplicationUserId, int count)
        {
            // Atualiza UnReadChat do Agente para o qual a mensagem foi enviada
            UnReadChat unReadChat = await _ApplicationDbContext.UnReadChats
                                .Where(p => p.FromApplicationUserId == fromApplicationUserID & p.ToApplicationUserId == toApplicationUserId)
                                .FirstOrDefaultAsync();

            // Se ainda não tem registro ...
            if (unReadChat == null)
            {
                // Insere um registro
                unReadChat = new UnReadChat { FromApplicationUserId = fromApplicationUserID, ToApplicationUserId = toApplicationUserId, count = count, LastDateTime = DateTime.Now };
                _ApplicationDbContext.UnReadChats.Add(unReadChat);
            }
            else
            {
                // Atualiza o registro
                unReadChat.count += count;
                unReadChat.LastDateTime = Utility.HoraLocal();
                _ApplicationDbContext.UnReadChats.Update(unReadChat);
            }
        }
        /*
        *Getting Agent and Contact List Json
        * <SelectedId>      Id of Agent or Contact selected
        * <SelectedType>    1- SelectedID is Contact; 2- SelectedID is Agent
        * <lastTime>        last chattinglog Time that was already retreaved; will show records greater than that        * 
        * Tambem esta buscando os dados do ChatRoom - log da conversa do cliente ou agente selecionado
        */
        [HttpPost]
        public async Task<JsonResult> GetSideBarAsync(string selectedId, int selectedType, DateTime? lastTime, DateTime? lastActivity)
        {

            // Lista de mensagens com status alterados
            List<ChattingLog> changedStatusList = await GetChangedSatusList(selectedId, selectedType);

            // Lista de NOVAS mensagens com todos os dados relacionados, para ser mostrados no MsgHistory
            List<ChattingLogView> chattinglogViewList = await GetChattingLogViewList(selectedId, selectedType, lastTime);

            // Busca a lista de agentes 
            List<AgentView> agentList = await GetAgentList(lastActivity);

            // Busca pelos contatos 
            List<ContactView> contactList = await GetContactList(lastActivity).ConfigureAwait(false);

            // Devolve todos os objetos montados antes
            return Json(new
            {
                contactList,
                agentList,
                chattinglogViewList,
                changedStatusList
            }); ;
        }

        /*
        * Get Searched Message List
        */
        [HttpPost]
        public async Task<JsonResult> GetSearchedMessages(string searchKey)
        {

            // Procura as mensagens que batem com o SearchKey
            List<ChattingLog> searchedMessages = await SearchChattingLogs(searchKey).ConfigureAwait(false);

            // Devolve todos os objetos montados antes
            return Json(new
            {
                searchedMessages
            });
        }
        /*
        * Get Searched Contact List
        */
        [HttpPost]
        public async Task<JsonResult> GetSearchedContacts(string searchKey)
        {

            // Procura as mensagens que batem com o SearchKey
            List<ContactView> searchedContactList = await SearchContacts(searchKey).ConfigureAwait(false);

            // Devolve todos os objetos montados antes
            return Json(new
            {
                searchedContactList
            });
        }
        /*
        * Get Single Contact List
        */
        [HttpPost]
        public async Task<JsonResult> GetSingleContact(string searchId)
        {

            // Procura as mensagens que batem com o SearchKey
            List<ContactView> singleContactList = await SingleContact(searchId).ConfigureAwait(false);

            // Devolve todos os objetos montados antes
            return Json(new
            {
                singleContactList
            });
        }
        /*
         * Devolve lista com as mensagens recebidas após o usuario ter sido selecionado
         */
        public async Task<List<ChattingLogView>> GetChattingLogViewList(string selectedId, int selectedType, DateTime? lastTime)
        {

            // Devolve a lista de NOVAS mensagens com todos os dados relacionados, para ser mostrados no MsgHistory
            List<ChattingLogView> chattingLogViewList = new List<ChattingLogView>();

            // File Container URL
            var FileContainerUrl = _configuration.GetValue<string>($"FileContainerUrl");

            try
            {

                // A busca de novas mensagens só é feita se foi passado um selectedId
                if (!string.IsNullOrEmpty(selectedId) )
                {
                    // pra fazer o primeiro nivel do query na busca das mensagens
                    IQueryable<ChattingLogView> Query;

                    // Contact is selected
                    if (selectedType == 1)
                    {
                        Query = from a in _ApplicationDbContext.Set<ChattingLog>()
                                .Where(u => u.ContactId == selectedId & u.Source != MessageSource.Agent)
                                .Where(u => lastTime == null | u.Time > lastTime)
                                .Include(c => c.Contact)
                                .OrderBy(u => u.Time)
                                join b in _ApplicationDbContext.Set<ApplicationUser>()
                                on a.ApplicationUserId equals b.Id into g
                                from b in g.DefaultIfEmpty()
                                join e in _ApplicationDbContext.Set<ChattingLog>() on a.QuotedActivityId equals e.ActivityId into es
                                from e in es.DefaultIfEmpty()
                                select new ChattingLogView
                                {
                                    ChattingLog = new ChattingLog
                                    {
                                        ActivityId = a.ActivityId,
                                        ApplicationUserId = a.ApplicationUserId,
                                        ChatChannelId = a.ChatChannelId,
                                        ContactId = a.ContactId,
                                        FailedReason = a.FailedReason,
                                        Filename = String.IsNullOrEmpty(a.Filename) ? string.Empty : Utility.CombineUrlsToString(FileContainerUrl, a.Filename),
                                        GroupId = a.GroupId,
                                        Id = a.Id,
                                        IsHsm = a.IsHsm,
                                        Type = a.Type,
                                        Source = a.Source,
                                        Text = a.Text,
                                        Read = a.Read,
                                        Time = a.Time,
                                        QuotedActivityId = a.QuotedActivityId,
                                        Status = a.Status,
                                        StatusTime = a.StatusTime,
                                        ToAgentId = a.ToAgentId
                                    },
                                    ContactName = a.Contact.Name,
                                    AgentName = b.UserName,
                                    QuotedLog = e
                                };

                    }
                    // Agent is selected
                    else
                    {
                        string toUserId = _userManager.GetUserId(User);
                        Query = from a in _ApplicationDbContext.Set<ChattingLog>()
                                .Where(u => u.ApplicationUserId == selectedId & u.ToAgentId == toUserId )
                                .Where(u => lastTime == null | u.Time > lastTime)
                                .OrderBy(u => u.Time)
                                join b in _ApplicationDbContext.Set<Contact>() on a.ContactId equals b.Id into bs
                                from b in bs.DefaultIfEmpty()
                                join c in _ApplicationDbContext.Set<ApplicationUser>() on a.ApplicationUserId equals c.Id into cs
                                from c in cs.DefaultIfEmpty()
                                join d in _ApplicationDbContext.Set<ApplicationUser>() on a.ToAgentId equals d.Id into ds
                                from d in ds.DefaultIfEmpty()
                                join e in _ApplicationDbContext.Set<ChattingLog>() on a.QuotedActivityId equals e.ActivityId into es
                                from e in es.DefaultIfEmpty()
                                select new ChattingLogView
                                {
                                    ChattingLog = new ChattingLog 
                                    { 
                                        ActivityId = a.ActivityId,
                                        ApplicationUserId = a.ApplicationUserId,
                                        ChatChannelId = a.ChatChannelId,
                                        ContactId = a.ContactId,
                                        FailedReason = a.FailedReason,
                                        Filename = String.IsNullOrEmpty(a.Filename) ? string.Empty : Utility.CombineUrlsToString(FileContainerUrl, a.Filename),
                                        GroupId = a.GroupId,
                                        Id = a.Id,
                                        IsHsm = a.IsHsm,
                                        Type = a.Type,
                                        Source = a.Source,
                                        Text = a.Text,
                                        Read = a.Read,
                                        Time = a.Time,
                                        QuotedActivityId = a.QuotedActivityId,
                                        Status = a.Status,
                                        StatusTime = a.StatusTime,
                                        ToAgentId = a.ToAgentId
                                    },
                                    ContactName = a.ContactId == null ? "" : b.Name,
                                    AgentName = a.ApplicationUserId == null || string.IsNullOrEmpty(a.ApplicationUserId) ? string.Empty : c.FullName,
                                    ToAgentName = a.ToAgentId == null || string.IsNullOrEmpty(a.ToAgentId) ? string.Empty : d.FullName,
                                    QuotedLog = e,
                                };

                        // Zera UnReadMessages do Agente selecionado
                        UnReadChat unReadChat = await _ApplicationDbContext.UnReadChats
                                                .Where(p => p.FromApplicationUserId == selectedId & p.ToApplicationUserId == _userManager.GetUserId(User))
                                                .FirstOrDefaultAsync();
                        if (unReadChat != null)
                        {
                            unReadChat.count = 0;
                            unReadChat.LastDateTime = Utility.HoraLocal();
                            _ApplicationDbContext.UnReadChats.Update(unReadChat);
                            await _ApplicationDbContext.SaveChangesAsync();
                        }
                    }

                    // Converte o Query para List
                    chattingLogViewList = await Query.ToListAsync().ConfigureAwait(false);

                }
            }
            catch (Exception e)
            {
                Console.Write(e);

            }

            return chattingLogViewList;
        }
        /*
         * Devolve lista com as mensagens que tiveram o Status alterado nos ultimos 10 segundos
         */
        public async Task<List<ChattingLog>> GetChangedSatusList(string selectedId, int selectedType)
        {
            // Devolve a lista de mensagens com status alterados
            DateTime lastStatusTime = Utility.HoraLocal().AddSeconds(-10);
            List<ChattingLog> changedStatusList = new List<ChattingLog>();

            try
            {

                // A busca de novas mensagens só é feita se foi passado um selectedId e não tem um searchKey
                if (!string.IsNullOrEmpty(selectedId))
                {

                    // Contact is selected
                    if (selectedType == 1)
                    {

                        // Busca status alterados
                        changedStatusList = await _ApplicationDbContext.ChattingLogs.
                            Where(p => p.ContactId == selectedId && (p.Source == MessageSource.Bot || p.Source == MessageSource.Agent) && p.StatusTime > lastStatusTime)
                            .ToListAsync()
                            .ConfigureAwait(false);

                    }
                    // Agent is selected
                    else
                    {
                        // Busca status alterados nos ultimos N segundos
                        changedStatusList = await _ApplicationDbContext.ChattingLogs
                            .Where(p => p.ApplicationUserId == selectedId && (p.Source == MessageSource.Bot || p.Source == MessageSource.Agent) & p.StatusTime > lastStatusTime)
                            .ToListAsync()
                            .ConfigureAwait(false);

                    }

                    // Remove o Contact descriptor para evitar bug ao serializar
                    foreach (ChattingLog chat in changedStatusList)
                    {
                        chat.Contact = null;
                    }

                }
            }
            catch (Exception e)
            {
                Console.Write(e);

            }

            return changedStatusList;
        }
        /*
        *  Devolve uma lista com um unico contato - do Id indicad
        */
        public async Task<List<ContactView>> SingleContact(string searchId)
        {
            int roleNumber = await GetLoggedUserRoleNumber();

            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            List<ContactView> contacts;

            contacts = await (from p in _ApplicationDbContext.Contacts
                              where p.GroupId == groupId
                                  & p.Id == searchId
                              select new ContactView
                              {
                                  Id = p.Id,
                                  Name = string.IsNullOrEmpty(p.Name) ? "?" : p.Name,
                                  Avatar = string.IsNullOrEmpty(p.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFileName),
                                  FullName = p.FullName ?? string.Empty,
                                  NickName = p.NickName ?? p.Name,
                                  Email = p.Email,
                                  Phone = p.MobilePhone,
                                  Roles = "contact",
                                  Status = p.Status,
                                  LastMessage = p.LastText,
                                  LastTime = p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                  UnAnsweredCount = p.UnAnsweredCount,
                                  ChannelType = p.ChannelType,
                                  ApplicationUserId = p.ApplicationUserId
                              })
                                .ToListAsync();

            return contacts;

        }
        /*
        *  Devolve lista de contatos para o left side bar
        *  Primeiros 50 Contatos com atividade mais recente
        *  Se houver LastActivity, apenas os que tiveram atividade depois da data indicada - mais recentes
        */
        public async Task<List<ContactView>> GetContactList(DateTime? lastActivity)
        {
            int roleNumber = await GetLoggedUserRoleNumber();

            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            List<ContactView> contacts;

            contacts = await (from p in _ApplicationDbContext.Contacts
                                where p.GroupId == groupId
                                    & (roleNumber < 4 || (String.IsNullOrEmpty(p.ApplicationUserId) & roleNumber == 4) || p.ApplicationUserId.Equals(_userManager.GetUserId(User)))
                                    & (lastActivity == null || p.LastActivity > lastActivity)
                                orderby p.LastActivity descending
                                select new ContactView
                                {
                                    Id = p.Id,
                                    Name = string.IsNullOrEmpty(p.Name) ? "?" : p.Name,
                                    Avatar = string.IsNullOrEmpty(p.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFileName),
                                    FullName = p.FullName ?? string.Empty,
                                    NickName = p.NickName ?? p.Name,
                                    Email = p.Email,
                                    Phone = p.MobilePhone,
                                    Roles = "contact",
                                    Status = p.Status,
                                    LastMessage = p.LastText,
                                    LastTime = p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                    UnAnsweredCount = p.UnAnsweredCount,
                                    ChannelType = p.ChannelType,
                                    ApplicationUserId = p.ApplicationUserId,
                                    LastActivity = p.LastActivity
                                })
                                .Take(50)
                                .ToListAsync();

            return contacts;

        }
        /*
        *  Devolve lista de contatos para o left side bar
        *  Primeiros 50 Contatos com atividade mais recente
        *  Os que tiveram atividade ANTES da data indicada - append to left side bar
        */
        public async Task<List<ContactView>> AppendContactList(DateTime firstActivity)
        {
            int roleNumber = await GetLoggedUserRoleNumber();

            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            List<ContactView> contacts;

            contacts = await (from p in _ApplicationDbContext.Contacts
                              where p.GroupId == groupId
                                  & (roleNumber < 4 || (String.IsNullOrEmpty(p.ApplicationUserId) & roleNumber == 4) || p.ApplicationUserId.Equals(_userManager.GetUserId(User)))
                                  & p.LastActivity < firstActivity
                              orderby p.LastActivity descending
                              select new ContactView
                              {
                                  Id = p.Id,
                                  Name = string.IsNullOrEmpty(p.Name) ? "?" : p.Name,
                                  Avatar = string.IsNullOrEmpty(p.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFileName),
                                  FullName = p.FullName ?? string.Empty,
                                  NickName = p.NickName ?? p.Name,
                                  Email = p.Email,
                                  Phone = p.MobilePhone,
                                  Roles = "contact",
                                  Status = p.Status,
                                  LastMessage = p.LastText,
                                  LastTime = p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                  UnAnsweredCount = p.UnAnsweredCount,
                                  ChannelType = p.ChannelType,
                                  ApplicationUserId = p.ApplicationUserId,
                                  LastActivity = p.LastActivity
                              })
                                .Take(50)
                                .ToListAsync();

            return contacts;

        }
        /*
        *  Devolve lista de contatos que preenchem o critério de search
        */
        public async Task<List<ContactView>> SearchContacts(string searchKey)
        {
            int roleNumber = await GetLoggedUserRoleNumber();

            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            List<ContactView> contacts;

            contacts = await (from p in _ApplicationDbContext.Contacts
                              where p.GroupId == groupId
                                  &
                                  (string.IsNullOrEmpty(searchKey) || p.FullName.Contains(searchKey) || p.MobilePhone.Contains(Utility.ClearStringNumber(searchKey)) || p.Email.Contains(searchKey))
                                  &
                                  (roleNumber < 4 || (String.IsNullOrEmpty(p.ApplicationUserId) & roleNumber == 4) || p.ApplicationUserId.Equals(_userManager.GetUserId(User)))
                              orderby (p.Status == ContactStatus.WatingForAgent | p.UnAnsweredCount > 0) descending, p.LastActivity descending
                              select new ContactView
                              {
                                  Id = p.Id,
                                  Name = string.IsNullOrEmpty(p.Name) ? "?" : p.Name,
                                  Avatar = string.IsNullOrEmpty(p.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFileName),
                                  FullName = p.FullName ?? string.Empty,
                                  NickName = p.NickName ?? p.Name,
                                  Email = p.Email,
                                  Phone = p.MobilePhone,
                                  Roles = "contact",
                                  Status = p.Status,
                                  LastMessage = p.LastText,
                                  LastTime = p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                  UnAnsweredCount = p.UnAnsweredCount,
                                  ChannelType = p.ChannelType,
                                  ApplicationUserId = p.ApplicationUserId
                              })
                                .Take(50)
                                .ToListAsync();

            return contacts;

        }
        /*
        *  Devolve a lista de mensagens que contem a palavra da busca - para a pesquisa
        *  Se foi informado um SelectedId - está vindo pela busca pela URL - neste caso devolve lista vazia
        */
        public async Task<List<ChattingLog>> SearchChattingLogs(string searchKey)
        {
            // Só faz a pesquisa por mensagem se houver uma searchKey
            if (!string.IsNullOrEmpty(searchKey))
            {

                searchKey = searchKey.Replace("\"", "");
                List<ChattingLog> chattingLogView = await _ApplicationDbContext.ChattingLogs
                                        .Where(u => u.GroupId == _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId 
                                                & u.Text.Contains(searchKey)
                                                & u.ContactId != null )
                                        .Include(c => c.Contact)
                                        .OrderByDescending(o=> o.Time)
                                        .Take(50)
                                        .ToListAsync();

                return chattingLogView;
            }
            else
            {
                return new List<ChattingLog>();
            }
        }
        /*
        * Devolve a lista de agentes para a procura no left side bar
        *    Filtra os atendentes do grupo do usuario indicado em currentUserId
        *            -  tempo de atividade e mensagens não lidas depende da relação entre os atendentes, buscada em UnRadChats
        */
        public async Task <JsonResult> GetSearchedAgents(string SearchKey)
        {
            int groupId = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;
            var currentUserId = _userManager.GetUserId(User);

            var agentList = from p in _ApplicationDbContext.ApplicationUsers
                                            where p.Id != currentUserId & p.GroupId == groupId
                                            join q in _ApplicationDbContext.UnReadChats
                                                .Where(x => x.ToApplicationUserId == currentUserId) on p.Id equals q.FromApplicationUserId
                                                into qs
                                                from q in qs.DefaultIfEmpty()
                                            orderby p.LastActivity descending
                                            select new
                                            {
                                                Id = p.Id,
                                                ApplicationUserId = p.Id,
                                                Avatar = string.IsNullOrEmpty(p.PictureFile) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFile),
                                                FullName = p.FullName ?? string.Empty,
                                                NickName = p.NickName ?? string.Empty,
                                                Name = p.FullName ?? string.Empty,
                                                Email = p.Email ?? string.Empty,
                                                Phone = p.PhoneNumber,
                                                LastMessage = string.Empty,
                                                LastDateTime = q.LastDateTime,
                                                LastTime = p.LastActivity == null ? "&nbsp;" : (DateTime.Today - p.LastActivity).TotalDays > 365 * 10 ? string.Empty : p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                                UnAnsweredCount = q.count,
                                                DepartmentId = p.DepartmentId
                                            };

                // Filtra pelos critérios da busca
                agentList = agentList.Where(u => u.FullName.Contains(SearchKey) ||  u.NickName.Contains(SearchKey) );


            var searchedAgentList =  await agentList.ToListAsync();

            // Devolve todos os objetos montados antes
            return Json(new
            {
                searchedAgentList
            });

        }
        /*
        * Devolve a lista de agentes para o left side bar
        *    Filtra os atendentes do grupo do usuario indicado em currentUserId
        *    Filtra os atendentes que tiveram ativide em menos de uma hora, ou mensagens não lidas
        *            -  tempo de atividade e mensagens não lidas depende da relação entre os atendentes, buscada em UnRadChats
        *    Se tiver LastActivity, busca somente os que tem atividade depois da data especificada
        */
        public async Task<List<AgentView>> GetAgentList(DateTime? lastActivity)
        {
            // Id do usuario ( Agente ) logado
            string currentUserId = _userManager.GetUserId(User);

            try
            {
                IQueryable<AgentView> agentList = from p in _ApplicationDbContext.ApplicationUsers
                                                  where p.GroupId == _ApplicationDbContext.Users.Find(currentUserId).GroupId && (lastActivity == null || p.LastActivity > lastActivity)
                                                  join q in _ApplicationDbContext.UnReadChats
                                                      .Where(x => x.ToApplicationUserId == currentUserId ) on p.Id equals q.FromApplicationUserId
                                                     into qs
                                                  from q in qs.DefaultIfEmpty()
                                                  orderby p.LastActivity descending
                                                  select new AgentView
                                                  {
                                                      Id = p.Id,
                                                      ApplicationUserId = p.Id,
                                                      Avatar = string.IsNullOrEmpty(p.PictureFile) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), p.PictureFile),
                                                      FullName = p.FullName ?? string.Empty,
                                                      Name = p.FullName ?? string.Empty,
                                                      NickName = p.NickName ?? string.Empty,
                                                      Email = p.Email ?? string.Empty,
                                                      Phone = p.PhoneNumber,
                                                      LastMessage = string.Empty,
                                                      LastDateTime = q.LastDateTime,
                                                      LastTime = p.LastActivity == null ? "&nbsp;" : (DateTime.Today - p.LastActivity).TotalDays > 365 * 10 ? string.Empty : p.LastActivity.ToString("MM/dd/yyyy HH:mm:ss tt"),
                                                      UnAnsweredCount = q.count,
                                                      DepartmentId = p.DepartmentId
                                                  };

                // Deixa na lista quem teve atividade na ultima hora, ou mensagem não lida
                agentList = agentList.Where(q => q.LastDateTime > Utility.HoraLocal().AddMinutes(-60) | q.UnAnsweredCount > 0);

                return await agentList.ToListAsync();
            }

            catch (Exception)
            {
                return new List<AgentView>();
            }

        }
        /*
         *deleting a selected agent
         * <id> selected agent's id
         */
        [HttpPost]
		public async Task<ActionResult> DeleteAgent(string id)
		{

			ApplicationUser cuser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);

			_ApplicationDbContext.Remove(cuser);
			_ApplicationDbContext.SaveChanges();

			return Json(new
			{
				error = ""
			});
		}
		/*
        *updateing a selected agent profle information
        * <avatar>    vatar image file object
        * <id>        selected agent id
        * <fullname>  fullname
        * <nickname>  nickname is name displayed into UI
        * <username>  login user name
        */
		[HttpPost]
		public async Task<ActionResult> EditAgent(List<IFormFile> avatar, string id, string fullname, string nickname, string username, string newpassword, string role)
		{
            // Check if role exists
            List<ApplicationRole> roles = _ApplicationDbContext.ApplicationRoles.ToList();
			if (!roles.Any(r => r.Name == role))
				role = roles[0].Name;
            ApplicationUser cuser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            string filename = await _common.SaveUserAvatar(avatar, id, cuser.GroupId).ConfigureAwait(false);
            cuser.UserName = username;
            cuser.NormalizedUserName = string.IsNullOrEmpty(username) ? string.Empty : username.ToUpperInvariant();
            cuser.FullName = fullname;
            cuser.NickName = nickname;
            cuser.PictureFile = filename;

            // If new password was set
            if (!string.IsNullOrEmpty(newpassword))
                cuser.PasswordHash = _userManager.PasswordHasher.HashPassword(cuser, newpassword);

            _ApplicationDbContext.Entry(cuser).CurrentValues.SetValues(cuser);
			_ApplicationDbContext.SaveChanges();

            // If role was changed
            var oldRoleList = await _userManager.GetRolesAsync(cuser).ConfigureAwait(false);

			if (!oldRoleList.Contains(role))
			{
				await _userManager.RemoveFromRoleAsync(cuser, oldRoleList[0]).ConfigureAwait(false);
				await _userManager.AddToRoleAsync(cuser, role).ConfigureAwait(false);
			}

            _ApplicationDbContext.Entry(cuser).State = EntityState.Modified;

            return Json(new
			{
				name = username,
				avatar = filename
			});
		}
		/*
        * Edit a selected contact 
        * <avatar>    vatar image file object
        * <id>        selected agent id
        * <fullname>  fullname
        * <nickname>  nickname is name displayed into UI
        * <username>  login user name
        */
		[HttpPost]
        public async Task<ActionResult> EditContact(List<IFormFile> avatar, string id, string fullname, string nickname, string username, string resetpassword)
        {
            try
            {
                IQueryable<Contact> Query = _ApplicationDbContext.Contacts.Where(u => u.Id == id);
                if (Query.Any())
                {
                    Contact cuser = await _ApplicationDbContext.Contacts.FirstAsync().ConfigureAwait(false);
                    cuser.Name = username;
                    cuser.NickName = nickname;
                    cuser.FullName = fullname;
                    _ApplicationDbContext.Entry(cuser).CurrentValues.SetValues(cuser);
                    _ApplicationDbContext.Entry(cuser).State = EntityState.Modified;
                    _ApplicationDbContext.SaveChanges();
                }
            }
            catch (Exception e)
            { 
                Console.WriteLine(e.ToString()); 
            }
            return Json(new
            {
                error = "",
                name = username
            });
        }
        /*
        *updateing a selected contact profle information
        * from right detail panel
        */
        [HttpPost]
        public async Task<ActionResult> EditUserFromRightPanel(
            int type, string id, string fullname, string email, string phone,
            ChannelType channel,string agent)
        {
            string error = "";
            try
            {
                if (type==0)
                {
                    ApplicationUser user = await _ApplicationDbContext.ApplicationUsers.FindAsync(id).ConfigureAwait(false);
                    user.FullName = fullname;
                    user.Email = email;
                    user.NormalizedEmail = string.IsNullOrEmpty(email) ? string.Empty : email.ToUpperInvariant();
                    user.PhoneNumber = phone;
                    _ApplicationDbContext.Entry(user).CurrentValues.SetValues(user);
                    _ApplicationDbContext.Entry(user).State = EntityState.Modified;
                    _ApplicationDbContext.SaveChanges();
                }
                else {
                    Contact user = await _ApplicationDbContext.Contacts.FindAsync(id).ConfigureAwait(false);
                    user.FullName = fullname;
                    user.Name = Utility.FirstName(fullname);
                    user.Email = email;
                    user.MobilePhone = phone;
                    user.ChannelType = channel;
                    if ( agent != null && agent != "null" && user.ApplicationUserId != null && user.ApplicationUserId != "null" && user.ApplicationUserId != agent) {
                        // Sends WebPush Notification for the destination Agent
                        ApplicationUser fromAgent = await _ApplicationDbContext.ApplicationUsers.FindAsync(user.ApplicationUserId).ConfigureAwait(false);
                        ApplicationUser toAgent = await _ApplicationDbContext.ApplicationUsers.FindAsync(agent).ConfigureAwait(false);
                        await _wpushClient.SendNotification(
                            $"{fromAgent.NickName} is changed as {toAgent.NickName} associated with contact {user.Name}",
                            $"{fromAgent.NickName} transferred you contact {user.Name}",
                            _configuration.GetValue<string>($"ContactCenterUrl"),
                        toAgent.WebPushId).ConfigureAwait(false);
                    }
                    user.ApplicationUserId = agent;

                    if (user.Status == ContactStatus.WatingForAgent)
                        user.Status = ContactStatus.TalkingToAgent;
                    user.UnAnsweredCount = 0;

                    _ApplicationDbContext.Entry(user).CurrentValues.SetValues(user);
                    _ApplicationDbContext.Entry(user).State = EntityState.Modified;
                    _ApplicationDbContext.SaveChanges();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                error = e.ToString();
            }
            return Json(new
            {
                error = error,
            });
        }

		/**
         * saving webpush subscribe  to user table
         */
        [HttpPost,HttpGet]
        public async Task<ActionResult> SaveWebPushIdAsync(string webpushId) {
            try
            {
                if ( webpushId != "false")
				{
                    ApplicationUser applicationUser = await _userManager.GetUserAsync(User).ConfigureAwait(false);
                    applicationUser.WebPushId = webpushId;

                    _ApplicationDbContext.Entry(applicationUser).CurrentValues.SetValues(applicationUser);
                    _ApplicationDbContext.Entry(applicationUser).State = EntityState.Modified;
                    _ApplicationDbContext.SaveChanges();
                }
            } 
            
            catch ( Exception e)
            {
                return Json(new
                {
                    error = e.Message
                });
            }
            return Json(new
            {
                error = ""
            });
        }
        
        /*
        *error page's action
        */
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}