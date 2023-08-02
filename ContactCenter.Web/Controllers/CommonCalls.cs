using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;

namespace ContactCenter.Controllers
{
    public class CommonCalls
    {
        private readonly ApplicationDbContext _ApplicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly IConfiguration _configuration;
        private const string avapath = "/assets-chatroom/images/avatars/default.png";
        public CommonCalls(IConfiguration configuration, ApplicationDbContext ApplicationDbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, BlobContainerClient blobContainerClient)
        {
            _ApplicationDbContext = ApplicationDbContext;
            _userManager = userManager;
            _environment = environment;
            _blobContainerClient = blobContainerClient;
            _configuration = configuration;
        }
        /*
        * getting current logined user/agent object.
        * This object is used in layout's header area.
        */
        public async Task<AgentView> GetCurrentUser(string cuserid)
        {

            AgentView agent = null;
            ApplicationUser user = await _userManager.Users.Where(u => u.Id == cuserid).FirstOrDefaultAsync().ConfigureAwait(false);
            if ( user != null )
            {
                agent = new AgentView();
                agent.Id = user.Id;
                agent.Avatar = (!string.IsNullOrEmpty(user.PictureFile)) ? user.PictureFile : avapath;
                agent.Name = string.IsNullOrEmpty(user.UserName) ? "&nbsp;" : user.UserName;
                agent.FullName = user.FullName;
                agent.NickName = user.NickName;
                agent.Email = user.Email;
                agent.Phone = user.PhoneNumber;
                agent.Roles = _userManager.GetRolesAsync(user).Result.First();
                agent.LastMessage = string.IsNullOrEmpty(user.LastText) ? "&nbsp;" : user.LastText;
                agent.LastTime = user.LastActivity.ToString(new CultureInfo("pt-BR"));
            }

            //Insere o caminho do Avatar
            if (!string.IsNullOrEmpty(agent.Avatar) && !agent.Avatar.StartsWith("/assets-chatroom/"))
                agent.Avatar = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), agent.Avatar);
            else if (string.IsNullOrEmpty(agent.Avatar))
                agent.Avatar = "NULL";

            return agent;
        }

        /*
         *check administrator 
         */
        private bool IsAdministrator(string userid)
        {
            IList<ApplicationUser> adminusers = _userManager.GetUsersInRoleAsync("sysadmin").Result;
            foreach (ApplicationUser adminuser in adminusers)
            {
                if (adminuser.Id == userid)
                {
                    return true;
                }
            }
            adminusers = _userManager.GetUsersInRoleAsync("groupadmin").Result;
            foreach (ApplicationUser adminuser in adminusers)
            {
                if (adminuser.Id == userid)
                {
                    return true;
                }
            }
            return false;
        }
        /**
         * convert long text to kindly view text in left panel's last message
        */
        public static string ShortenMessageText(string s)
        {
            var len = 35;
            var arr = s.Split("\n");
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i].Length == 0) continue;
                s = arr[i];
                if (s.Length > len) s = s.Substring(0, len - 3) + "...";
                return s;
            }
            return s;
        }
        /*
         * SaveUserAvatar
         */
        public async Task<string> SaveUserAvatar(List<IFormFile> avatar, string id, int groupId) {

            string filename=string.Empty;

            // Se veio Id
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    ApplicationUser cuser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
                    if (cuser != null)
                        filename = cuser.PictureFile;
                }
                catch (Exception) { }
            }

            // Se veio imagem
            if (avatar != null && avatar.Count > 0 && !string.IsNullOrEmpty(avatar[0].FileName))
            {
                // Confere que temos acesso ao Blob Storage
                if (_blobContainerClient != null)
                {
                    // Deleta o arquivo anterior
                    if (!string.IsNullOrEmpty(filename))
                    {
                        BlobClient blob = _blobContainerClient.GetBlobClient(filename);
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                    }

                    // Gera nome unico
                    filename = Utility.UniqueFileName(id + Path.GetExtension(avatar[0].FileName), groupId);

                    // Cria Stream em memória para trabalhar a imagem
                    using (Stream memoryStream = new MemoryStream())
                    {
                        // Copia a imagem recebida do formulario para o stream
                        await avatar[0].CopyToAsync(memoryStream).ConfigureAwait(false);

                        // Se for JPG
                        if (filename.EndsWith("jpg", StringComparison.InvariantCulture))
                        {
                            // Se for maior que 512px, reduz a resolução
                            memoryStream.Position = 0;
                            using (var image = new MagickImage(memoryStream))
                            {
                                if (image.BaseWidth > 512)
                                {
                                    image.Resize(512, 0);
                                    image.Write(memoryStream);
                                }
                            }
                        }

                        // Salva no Storage
                        memoryStream.Position = 0;
                        BlobClient blob = _blobContainerClient.GetBlobClient(filename);
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                        await blob.UploadAsync(memoryStream).ConfigureAwait(false);
                    }
                }
            }

            return filename;
        }
    }
}
