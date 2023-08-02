using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagesController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobContainerClient _blobContainerClient;

        public MessagesController(ApplicationDbContext context, BlobContainerClient blobContainerClient)
        {
            _context = context;
            _blobContainerClient = blobContainerClient;
        }
        // GET: api/<MessagesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages()
        {
            return await _context.Messages
                .Where(p => p.GroupId == AuthorizedGroupId() && p.MessageType != MessageType.grouplink)
                .Select(q => new MessageDto(q))
                .ToListAsync();
        }

        // GET api/<MessagesController>/Templates
        [HttpGet("templates")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetTemplates()
        {
            return await _context.Messages
                .Where(p => p.GroupId == 1)
                .Select(q => new MessageDto(q))
                .ToListAsync();
        }

        // GET api/<MessagesController>/Templates/Id
        [HttpGet("templates/{id}")]
        public async Task<ActionResult<MessageDto>> GetTemplate(int id)
        {
            // Find Message
            MessageDto MessageDto = await _context.Messages
                        .Where(p => p.Id == id & p.GroupId == 1)
                        .Select(p => new MessageDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (MessageDto == null)
            {
                return NotFound();
            }
            else
            {
                return MessageDto;
            }
        }


        // GET api/<MessagesController>/code/index
        [HttpGet("code/{index}")]
        public ActionResult GetCode(int index)
        {
            Message message = new Message();
            return Ok(message.CodeIndex(index));
        }

        // GET api/<MessagesController>/uncode/code
        [HttpGet("uncode/{code}")]
        public ActionResult GetIndex(string code)
        {
            Message message = new Message();
            return Ok(message.UnCodeIndex(code));
        }


        // GET api/<MessagesController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            // Find Message
            MessageDto MessageDto = await _context.Messages
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select(p => new MessageDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (MessageDto == null)
            {
                return NotFound();
            }
            else
            {
                return MessageDto;
            }
        }

        // POST api/<MessagesController>
        [HttpPost]
        public async Task<ActionResult<MessageDto>> PostMessage(Message message)
        {

            // Bind Group
            message.GroupId = AuthorizedGroupId();

            // Set last activity to now so Contact appears up at left sidebar
            message.CreatedDate = Utility.HoraLocal();

            // If messageType is email/smartpage
            if ( message.MessageType == MessageType.email )
            {
                // Get Last smartPage index used at messages
                int smartIndex = _context.Messages.Select(p => p.SmartIndex).Max()??0;

                // Generate next smart code
                smartIndex++;
                message.SmartCode = message.CodeIndex(smartIndex);
                message.SmartIndex = smartIndex;
            }

            // Add 
            _context.Messages.Add(message);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MessageExists(message.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("PostMessage", new { id = message.Id }, new MessageDto(message));
        }

        // PUT api/<MessagesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            // Check if Message has Id
            if (id != message.Id | id==0)
            {
                string error = "Não foi informado uma Messagem válida com seu ID.";
                return BadRequest(error);
            }

            // Check if Message Id existe at database
            Message oldMessage = await _context.Messages
                                .Where(p=> p.Id == id)
                                .FirstOrDefaultAsync();

            if (oldMessage == null)
            {
                string error = "Messagem {id} não localizada na base.";
                return NotFound(error);
            }

            // Se tinha outro arquivo salvo antes
            if (oldMessage.FileUri != null && oldMessage.FileUri != message.FileUri)
            {
                // Deleta o arquivo
                await DeleteFile(oldMessage.FileUri);
            }

            // Bind values - only when not null - like PATCH
            if ( message.MessageType != null)
                oldMessage.MessageType = message.MessageType;
            if ( message.Title != null)
                oldMessage.Title = message.Title;
            if (message.Content != null)
                oldMessage.Content = message.Content;
            if (message.Html != null)
                oldMessage.Html = message.Html;
            if (message.FileUri != null)
                oldMessage.FileUri = message.FileUri;

            // Update Database
            _context.Update(oldMessage);
            await _context.SaveChangesAsync();

            // Return
            return NoContent();
        }

        // DELETE api/<MessagesController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MessageDto>> Delete(int id)
        {
            var message = await _context.Messages
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            // Se tem arquivo
            if ( message.FileUri != null)
			{
                // Deleta o arquivo
                await DeleteFile(message.FileUri);
			}

            try
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return new MessageDto(message);
            }
            catch ( Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }

        private async Task DeleteFile(Uri fileName)
        {
            // Confere se recebeu arquivo
            if (fileName != null)
            {
                // Monta o nome, com o caminho do grupo
                string groupFileName = $"{AuthorizedGroupId()}/{fileName}";

                // Confere se temos aceso ao blobContainer
                if (_blobContainerClient != null)
                {
                    // Aponta para o arquivo
                    BlobClient blob = _blobContainerClient.GetBlobClient(groupFileName);
                    // Confere se o arquivo existe
                    if (await blob.ExistsAsync())
                    {
                        // Exclui o arquivo
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                    }
                }
                else
                    // Erro na inicialização do Blob Storage
                    throw (new SystemException("_blobContainerClient nulo"));
            }

        }
    }
}
