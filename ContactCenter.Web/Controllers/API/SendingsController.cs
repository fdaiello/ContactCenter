using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Infrastructure.Utilities;
using RestSharp;
using System.Text.RegularExpressions;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SendingsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SendingsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        // GET: api/<SendingsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SendingDto>>> GetSendings()
        {
            var sendings = await _context.Sendings
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Include(p => p.Board)
                .Select(q => new SendingDto(q))
                .ToListAsync();

            return sendings;
        }

        // GET api/<SendingsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SendingDto>> GetSending(int id)
        {
            // Find Sending
            SendingDto SendingDto = await _context.Sendings
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select(p => new SendingDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (SendingDto == null)
            {
                return NotFound();
            }
            else
            {
                return SendingDto;
            }
        }

        // POST api/<SendingsController>
        [HttpPost]
        public async Task<ActionResult<SendingDto>> PostSending(Sending sending)
        {

            // Bind Group
            sending.GroupId = AuthorizedGroupId();

            // Set last activity to now so Contact appears up at left sidebar
            sending.CreatedDate = Utility.HoraLocal();

            // Se a hora agendada vier em branco, salva com hora atual
            if (sending.ScheduledDate == null)
                sending.ScheduledDate = Utility.HoraLocal();

            // Confere se tem smart-page e amarra o Id
            sending.SmartPageId = await SearchSmartPageId(sending);

            // Procura nas campanhas de Grupo, para ver se a lista usada neste envio, é uma lista de grupos de GroupLInk
            GroupCampaign groupCampaign = await _context.GroupCampaigns
                                        .Where(p => p.GroupBoardId == sending.BoardId)
                                        .FirstOrDefaultAsync();
            // Se achou
            if (groupCampaign != null)
                // O envio tem que ficar no mesmo canal da campanha
                sending.ChatChannelId = groupCampaign.ChatChannelId;

            // Add 
            _context.Sendings.Add(sending);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SendingExists(sending.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            // If need to send right now
            DateTime scheduledDate = sending.ScheduledDate ?? Utility.HoraLocal();
            if (Utility.HoraLocal().Subtract(scheduledDate).TotalSeconds > 0)
			{
                // Insert object at queue to fire BulkSending
                await QueueBulkSending(sending);
            }

            // Query to get descriptors
            Sending newSending = await _context.Sendings
                                .Where(p => p.Id == sending.Id)
                                .Include( b=> b.Board)
                                .Include( m=> m.Message)
                                .Include( c=> c.ChatChannel)
                                .FirstOrDefaultAsync();
                            
            return CreatedAtAction("PostSending", new { id = sending.Id }, new SendingDto(newSending));
        }

        // PUT api/<SendingsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSending(int id, Sending sending)
        {
            // Check if Sending has Id
            if (id != sending.Id | id==0)
            {
                string error = "Não foi informado um Envio (Sending) válido com seu ID.";
                return BadRequest(error);
            }

            // Check if Sending Id existe at database
            Sending oldSending = await _context.Sendings
                                .Where(p=> p.Id == id)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            // Check if sending exists
            if (oldSending == null)
            {
                string error = "Envio (Sending) Id {id} não localizado na base.";
                return NotFound(error);
            }
            // Check if it has not been sent 
            else if ( oldSending.Status != SendingStatus.Queued)
			{
                string error = $"Este envio não pode mais ser alterado pois está com status {oldSending.Status.Description()}";
                return BadRequest(error);
            }

            // Procura nas campanhas de Grupo, para ver se a lista usada neste envio, é uma lista de grupos de GroupLInk
            GroupCampaign groupCampaign = await _context.GroupCampaigns
                                        .Where(p => p.GroupBoardId == sending.BoardId)
                                        .FirstOrDefaultAsync();
            // Se achou
            if (groupCampaign != null)
                // O envio tem que ficar no mesmo canal da campanha
                sending.ChatChannelId = groupCampaign.ChatChannelId;

            // Bind Group
            sending.GroupId = AuthorizedGroupId();

            // Created Date is coming null
            sending.CreatedDate = oldSending.CreatedDate;

            // Se a hora agendada vier em branco, salva com hora atual
            if (sending.ScheduledDate == null)
                sending.ScheduledDate = Utility.HoraLocal();

            // Update Database
            _context.Update(sending);
            await _context.SaveChangesAsync();

            // Return
            return NoContent();
        }

        // DELETE api/<SendingsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SendingDto>> Delete(int id)
        {
            var sending = await _context.Sendings
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (sending == null)
            {
                return NotFound();
            }
            // Check if it has not been sent 
            else if (sending.Status != SendingStatus.Queued)
            {
                string error = $"Este envio não pode mais ser excluido pois está com status {sending.Status.Description()}";
                return BadRequest(error);
            }

            _context.Sendings.Remove(sending);
            await _context.SaveChangesAsync();

            return new SendingDto(sending);


        }

        private bool SendingExists(int id)
        {
            return _context.Sendings.Any(e => e.Id == id);
        }

        private async Task QueueBulkSending( Sending sending )
		{
            // Remove filho pra nao gerar loop na desserialização
            if (sending.Message != null )
                sending.Message.Sendings = null;

            // Insere na fila de configuração
            string storageConnStr = _configuration.GetValue<string>("BlobStorageConnStr");
            QueueClient queue = new QueueClient(storageConnStr, "sendings");
            await queue.SendMessageAsync(Utility.Base64Encode(JsonConvert.SerializeObject(sending)));

        }
        private async Task<int?> SearchSmartPageId ( Sending sending)
		{
            int? smartPageId = null;

            // Se é envio de sms ou whatsapp
            if (sending.MessageType == MessageType.sms || sending.MessageType == MessageType.whatsapp)
            {
                // Busca a mensagem que foi usada neste envio
                Message message = await _context.Messages
                                   .Where(p => p.Id == sending.MessageId)
                                   .FirstOrDefaultAsync();
                // Se achou
                if (message != null)
                {
                    // Confere se esta mensaegm contém link para uma mensagem do tipo smart page
                    Match match = Regex.Match(message.Content, @"smart-page\.cc\/[a-zA-Z1-9]*");
                    // Se achou 
                    if ( match.Success)
					{
                        // Pega o código da smart page
                        string smartCode = match.Value.Replace("smart-page.cc/", "");
                        // Localiza a página no banco
                        Message smartPage = await _context.Messages
                                        .Where(p => p.SmartCode == smartCode)
                                        .FirstOrDefaultAsync();
                        // Se achou
                        if ( smartPage != null)
						{
                            // Devolve o Id da smart page
                            smartPageId = smartPage.Id;
						}
					}
                }
            }

            return smartPageId;
        }

    }
}
