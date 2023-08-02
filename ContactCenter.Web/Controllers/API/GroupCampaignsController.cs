using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using ContactCenter.Infrastructure.Clients.Wassenger;
using System.Net;
using System.IO;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GroupCampaignsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private WassengerClient _wassenger;

        public GroupCampaignsController(ApplicationDbContext context, IConfiguration configuration, WassengerClient wassenger)
        {
            _context = context;
            _configuration = configuration;
            _wassenger = wassenger;
        }
        // GET: api/<GroupCampaignsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupCampaignDto>>> GetGroupCampaigns()
        {
            List<GroupCampaignDto> groupCampaignsDto = await _context.GroupCampaigns
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Include(p=>p.WhatsGroups)
                .Select(q => new GroupCampaignDto(q))
                .ToListAsync();

            return groupCampaignsDto;
        }

        // GET api/<GroupCampaignsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupCampaignDto>> GetGroupCampaign(int id)
        {
            // Find GroupCampaign
            GroupCampaignDto GroupCampaignDto = await _context.GroupCampaigns
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select(p => new GroupCampaignDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (GroupCampaignDto == null)
            {
                return NotFound();
            }
            else
            {
                return GroupCampaignDto;
            }
        }
        // GET api/<GroupCampaignsController>/Id/PageViewsDay
        [HttpGet("{id}/PageViewsDay")]
        public async Task<ActionResult<List<GroupCampaignView>>> GetPageViewsDay(int id)
        {
            // SQL for query database proceadure
            string SQL = $"exec groupcampaignPageViewsProc @Id={id}";

            // Query View
            List<GroupCampaignView> GroupCampaignViews = await _context.GroupCampaignPageView
                        .FromSqlRaw(SQL)
                        .AsNoTracking()
                        .ToListAsync();

            return GroupCampaignViews;
        }

        // POST api/<GroupCampaignsController>
        [HttpPost]
        public async Task<ActionResult<GroupCampaignDto>> PostGroupCampaign(GroupCampaign groupCampaign)
        {

            // Valida se foi informado um canal
            if (string.IsNullOrEmpty(groupCampaign.ChatChannelId))
                return BadRequest("Não foi informado o Canal.");

            // Valida se o canal está no ar
            WaDevice waDevice = await _wassenger.GetDevice(groupCampaign.ChatChannelId);
            if (waDevice.session.status != "online")
                return BadRequest("O celular não está sincronizado com a API. Verifique o aparelho.");

            // Bind Group
            groupCampaign.GroupId = AuthorizedGroupId();

            // Set last activity to now so Contact appears up at left sidebar
            groupCampaign.CreatedDate = Utility.HoraLocal();

            // Valida se já existe uma página com este nome
            List<Message> messages = await _context.Messages
                                    .Where(p => p.SmartCode == groupCampaign.LinkSufix)
                                    .ToListAsync();
            if ( messages.Any())
            {
                return BadRequest("Já existe uma página com este link: {groupCampaign.LinkSufix}. Por favor escolha outra palavra para o seu link.");
            }


            // Cria e amarra a Message ( smart page )
            Message message = new Message { MessageType = MessageType.grouplink, CreatedDate = Utility.HoraLocal(), GroupId = AuthorizedGroupId(), SmartCode = groupCampaign.LinkSufix, Title = groupCampaign.Name };

            // Se o LinkSufix veio em branco
            if (string.IsNullOrEmpty(groupCampaign.LinkSufix))
            {
                // Get Last smartPage index used at messages
                int smartIndex = _context.Messages.Select(p => p.SmartIndex).Max() ?? 0;

                // Generate next smart code
                smartIndex++;
                message.SmartCode = message.CodeIndex(smartIndex);
                message.SmartIndex = smartIndex;
                groupCampaign.LinkSufix = message.SmartCode;
            }

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
            groupCampaign.MessageId = message.Id;

            // Status
            groupCampaign.Status = GroupCampaingStatus.initializing;

            // Se sendMsgChannelId for string vazia, altera para nulo
            if (string.IsNullOrEmpty(groupCampaign.SendMsgChatChannelId))
                groupCampaign.SendMsgChatChannelId = null;

            // Insere caminho da imagem
            if (!string.IsNullOrEmpty(groupCampaign.ImageFileName))
                groupCampaign.ImageFileName = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), AuthorizedGroupId().ToString() + "/" + groupCampaign.ImageFileName);

            // Add 
            _context.GroupCampaigns.Add(groupCampaign);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GroupCampaignExists(groupCampaign.Id))
                {
                    return Conflict();
                }
                else
                { 
                    throw;
                }
            }

            // Insere na fila de configuração
            string storageConnStr = _configuration.GetValue<string>("BlobStorageConnStr");
            QueueClient queue = new QueueClient(storageConnStr, "groupcampaign");
            await queue.SendMessageAsync(Utility.Base64Encode(JsonConvert.SerializeObject(groupCampaign)));

            // Devolve Ok com o objeto criado
            return CreatedAtAction("PostGroupCampaign", new GroupCampaignDto(groupCampaign));
        }

        // PUT api/<GroupCampaignsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<GroupCampaignDto>> PutGroupCampaign(int id, GroupCampaign groupCampaign)
        {
            // Check if GroupCampaign has Id
            if (id != groupCampaign.Id | id==0)
            {
                string error = "Não foi informado uma campanha válida com seu ID.";
                return BadRequest(error);
            }

            // Valida se foi informado um canal
            if (string.IsNullOrEmpty(groupCampaign.ChatChannelId))
                return BadRequest("Não foi informado o Canal.");

            // Valida se o canal está no ar
            WaDevice waDevice = await _wassenger.GetDevice(groupCampaign.ChatChannelId);
            if (waDevice.session.status != "online")
                return BadRequest("O celular não está sincronizado com a API. Verifique o aparelho.");

            // Check if GroupCampaign Id existe at database
            GroupCampaign oldGroupCampaign = await _context.GroupCampaigns
                                .Where(p=> p.Id == id)
                                .FirstOrDefaultAsync();

            // Check if groupCampaign exists
            if (oldGroupCampaign == null)
            {
                string error = "Envio (GroupCampaign) Id {id} não localizado na base.";
                return NotFound(error);
            }

            // Nâo permite alterar se estiver ainda inicializando
            if ( oldGroupCampaign.Status == GroupCampaingStatus.initializing)
                return BadRequest("Aguarde a inicialização da campanha para poder altera-la.");

            // Não permite alterar se ainda estiver processando alteração anterior
            if ( oldGroupCampaign.ChangeDescription | oldGroupCampaign.ChangeImage | oldGroupCampaign.ChangePermissions | oldGroupCampaign.Updating)
                return BadRequest("A campanha está sendo atualizada, por favor aguarde o processamento antes de fazer nova alteração.");

            // Não permite diminuir a quantidade de grupos
            if ( oldGroupCampaign.Groups > groupCampaign.Groups)
                return BadRequest("Não é possível diminuir a quantidade de grupos que já foram criados.");

            // Se trocou o sufixo do link
            if (oldGroupCampaign.LinkSufix != groupCampaign.LinkSufix)
            {
                // Valida se já existe uma página com este nome
                List<Message> messages = await _context.Messages
                                        .Where(p => p.SmartCode == groupCampaign.LinkSufix)
                                        .ToListAsync();
                if (messages.Any())
                {
                    return BadRequest($"Já existe uma página com este link: {groupCampaign.LinkSufix}. Por favor escolha outra palavra para o seu link.");
                }

                // Busca a mensagem ( SmartPage ) vinculada a esta campanha
                Message message = await _context.Messages
                                  .Where(p => p.Id == oldGroupCampaign.MessageId)
                                  .FirstOrDefaultAsync();
                if ( message != null)
                {
                    message.SmartCode = groupCampaign.LinkSufix;
                    _context.Messages.Update(message);
                }

            }

            // Se mudou a data de encerramento - valida o status
            if (groupCampaign.EndDate < Utility.HoraLocal())
                oldGroupCampaign.Status = GroupCampaingStatus.closed;
            else if (oldGroupCampaign.Status == GroupCampaingStatus.closed)
                oldGroupCampaign.Status = GroupCampaingStatus.active;

            // Se alterou a imagem e veio nome de arquivo sem o caminho
            if (!string.IsNullOrEmpty(groupCampaign.ImageFileName) && groupCampaign.ImageFileName.Split("/").Length==1)
                // Insere caminho da imagem
                groupCampaign.ImageFileName = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), AuthorizedGroupId().ToString() + "/" + groupCampaign.ImageFileName);

            // Se sendMsgChannelId for string vazia, altera para nulo
            if (string.IsNullOrEmpty(groupCampaign.SendMsgChatChannelId))
                groupCampaign.SendMsgChatChannelId = null;

            // Anota o que precisa ser alterado nos grupos
            string obs = string.Empty;
            if (oldGroupCampaign.ImageFileName != groupCampaign.ImageFileName)
			{
                // Desmarca a flag que indica que a imagem foi definida, em todos os registros dos grupos
                oldGroupCampaign.ChangeImage = true;
                List<WhatsGroup> whatsGroups = await _context.WhatsGroups
                                                .Where(p => p.GroupCampaignId == oldGroupCampaign.Id && p.Created)
                                                .ToListAsync();
                foreach ( WhatsGroup whatsGroup in whatsGroups)
				{
                    whatsGroup.ImageSet = false;
                    _context.WhatsGroups.Update(whatsGroup);
				}
            }       
            if (oldGroupCampaign.GroupAdmins != groupCampaign.GroupAdmins)
                oldGroupCampaign.ChangeAdmins = true;
            if (oldGroupCampaign.Permissions != groupCampaign.Permissions)
                oldGroupCampaign.ChangePermissions = true;
            if (oldGroupCampaign.Name != groupCampaign.Name || oldGroupCampaign.Description != groupCampaign.Description)
			{
                oldGroupCampaign.ChangeDescription = true;

                // Desmarca a flag que indica que a imagem foi definida, em todos os registros dos grupos
                oldGroupCampaign.ChangeImage = true;
                List<WhatsGroup> whatsGroups = await _context.WhatsGroups
                                                .Where(p => p.GroupCampaignId == oldGroupCampaign.Id && p.Created)
                                                .ToListAsync();
                foreach (WhatsGroup whatsGroup in whatsGroups)
                {
                    whatsGroup.DescriptionSet = false;
                    _context.WhatsGroups.Update(whatsGroup);
                }

            }


            // Anota se aumentou os grupos
            bool createGroups = oldGroupCampaign.Groups < groupCampaign.Groups;

            // Salva os campos alterados
            oldGroupCampaign.Name = groupCampaign.Name;
            oldGroupCampaign.Description = groupCampaign.Description;
            oldGroupCampaign.LinkSufix = groupCampaign.LinkSufix;
            oldGroupCampaign.ImageFileName = groupCampaign.ImageFileName;
            oldGroupCampaign.ChatChannelId = groupCampaign.ChatChannelId;
            oldGroupCampaign.ClosedUrl = groupCampaign.ClosedUrl;
            oldGroupCampaign.MaxClicksPerGroup = groupCampaign.MaxClicksPerGroup;
            oldGroupCampaign.Groups = groupCampaign.Groups;
            oldGroupCampaign.GroupAdmins = groupCampaign.GroupAdmins;
            oldGroupCampaign.FacePixelCode = groupCampaign.FacePixelCode;
            oldGroupCampaign.GoogleAdsCode = groupCampaign.GoogleAdsCode;
            oldGroupCampaign.EndDate = groupCampaign.EndDate;
            oldGroupCampaign.Permissions = groupCampaign.Permissions;
            oldGroupCampaign.Obs = obs;
            oldGroupCampaign.ErrorsCount = 0;
            oldGroupCampaign.WelcomeMessageId = groupCampaign.WelcomeMessageId;
            oldGroupCampaign.GroupWelcomeMessageId = groupCampaign.GroupWelcomeMessageId;
            oldGroupCampaign.LeaveMessageId = groupCampaign.LeaveMessageId;
            oldGroupCampaign.SendMsgChatChannelId = groupCampaign.SendMsgChatChannelId;
            oldGroupCampaign.GroupAction = groupCampaign.GroupAction;

            // Flag que indica se estava em erro
            bool wasInError = false;
            if (oldGroupCampaign.Status == GroupCampaingStatus.error)
            {
                oldGroupCampaign.Status = GroupCampaingStatus.active;
                wasInError = true;
            }

            // Update Database
            _context.Update(oldGroupCampaign);
            await _context.SaveChangesAsync();

            // Se alterou algo que deve ser refletido nos grupos, ou se estava em status de erro
            if (oldGroupCampaign.ChangeImage || oldGroupCampaign.ChangePermissions || oldGroupCampaign.ChangeDescription || wasInError || createGroups)
            {
                // Insere na fila de configuração
                string storageConnStr = _configuration.GetValue<string>("BlobStorageConnStr");
                QueueClient queue = new QueueClient(storageConnStr, "groupcampaign");
                await queue.SendMessageAsync(Utility.Base64Encode(JsonConvert.SerializeObject(oldGroupCampaign)));
            }

            // Return
            return Ok(new GroupCampaignDto(oldGroupCampaign));
        }

        // DELETE api/<GroupCampaignsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GroupCampaignDto>> Delete(int id)
        {
            var groupCampaign = await _context.GroupCampaigns
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (groupCampaign == null)
            {
                return NotFound();
            }

            // Se tinha smart page salva
            if (!string.IsNullOrEmpty(groupCampaign.LinkSufix))
            {
                // Localiza a smart page
                List<Message> messages = await _context.Messages
                                        .Where(p => p.SmartCode == groupCampaign.LinkSufix)
                                        .ToListAsync();

                foreach( Message message in messages)
                {
                    // Procura se tem envios que usam esta smart page
                    List<Sending> sendings = await _context.Sendings
                                            .Where(p => p.MessageId == message.Id)
                                            .ToListAsync();

                    if (sendings.Count() > 0)
                    {
                        foreach (Sending sending in sendings)
                            _context.Sendings.Remove(sending);
                    }

                    // Exclui a Smart Page vinculada a esta campanha;
                    _context.Messages.Remove(message);
                }

            }

            _context.GroupCampaigns.Remove(groupCampaign);
            await _context.SaveChangesAsync();

            return new GroupCampaignDto(groupCampaign);


        }

        private bool GroupCampaignExists(int id)
        {
            return _context.GroupCampaigns.Any(e => e.Id == id);
        }

    }
}
