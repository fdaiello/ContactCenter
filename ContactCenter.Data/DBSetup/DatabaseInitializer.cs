using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ContactCenter.Data.Interfaces;
using ContactCenter.Core.Models;

namespace ContactCenter.Data
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;

        public DatabaseInitializer(ApplicationDbContext context, IAccountManager accountManager, ILogger<DatabaseInitializer> logger)
        {
            _accountManager = accountManager;
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);

            if (!await _context.Users.AnyAsync().ConfigureAwait(false))
            {
                _logger.LogInformation(new string("Generating default accounts"));

                //define default roles
                const string sysadminRoleName = "sysadmin";                 //system administrator
                const string groupadminRoleName = "groupadmin";             //group administrotr
                const string agentRoleName = "agent";                       //agent role
                const string supervisorRoleName = "supervisor";             //Supervisor role
                const string atendenteRoleName = "atendente";               //Atendente role

                //when system is deployed to client at first, system generate detfault roles
                await EnsureRoleAsync(sysadminRoleName, "System administrator", ApplicationPermissions.GetAllPermissionValues()).ConfigureAwait(false);
                await EnsureRoleAsync(groupadminRoleName, "Group Administrator", ApplicationPermissions.GetAllPermissionValues()).ConfigureAwait(false);
                await EnsureRoleAsync(agentRoleName, "Default agent", Array.Empty<string>()).ConfigureAwait(false);
                await EnsureRoleAsync(atendenteRoleName, "Atendente", Array.Empty<string>()).ConfigureAwait(false);
                await EnsureRoleAsync(supervisorRoleName, "Default supervisor", Array.Empty<string>()).ConfigureAwait(false);
                /*
                 * when system is deployed to client at first, system generate 
                 * system administrator automatically.
                 * username: admin
                 * password: tempP@ss123
                 * email:    fdaiello@misterpostman.com.br
                 */
                await CreateUserAsync("sysadmin", "tempP@ss123", "System Administrator", "system_admin", "fdaiello@misterpostman.com.br", "+55 (51) 000-0000", new string[] { sysadminRoleName },0).ConfigureAwait(false);
                
                _logger.LogInformation(new string("default account generation completed"));
            }
        }
        /*
         * EnsureRoleAsync generate system user's roles.
         * params
         * roleName:     role name
         * description:  role description
         * claims:       role claims
         */
        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _accountManager.GetRoleByNameAsync(roleName).ConfigureAwait(false)) == null)
            {
                ApplicationRole applicationRole = new ApplicationRole(roleName, description);
                var result = await _accountManager.CreateRoleAsync(applicationRole, claims).ConfigureAwait(false);
                if (!result.Succeeded)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Errors)}");
            }
        }
        /*
         * CreateUserAsync generate system users with givien parameters such as username, email, password.
         * params
         * userName:     user name when log in to system
         * password:     password  when log in to system
         * fullName:     user full name
         * nickName:     user mickname
         * email:        user email when log in to system       
         * phoneNumber:  user phone number
         * roles:        role array
         * groupid:      group id
         */
        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string fullName, string nickName, string email, string phoneNumber, string[] roles,int groupid)
        {
            Group group = new Group
            {
                Name = "DefaultGroup"
            };

            await _context.Groups.AddAsync(group).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = userName,
                FullName = fullName,
                NickName = nickName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                IsEnabled = true,
                GroupId=1
            };
            var result = await _accountManager.CreateUserAsync(applicationUser, roles, password).ConfigureAwait(false);

            if (!result.Succeeded)
                throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Errors)}");
            return applicationUser;
        }
    }
}
