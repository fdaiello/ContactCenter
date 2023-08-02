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

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatChannelsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatChannelsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/<ChatChannelsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatChannelDto>>> GetChatChannels()
        {

            // Check authenticated user department
            ApplicationUser applicationUser = await _context.ApplicationUsers.FindAsync(AuthenticatedUserId());

            // Devolve os canais disponívis para o usuário autenticado - considerando o setor e o usuario do canal, se houver especificados
            return await _context.ChatChannels
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Select(q => new ChatChannelDto(q))
                .ToListAsync();
        }

        // GET api/<ChatChannelsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatChannelDto>> GetChatChannel(string id)
        {

            // Check authenticated user department
            ApplicationUser applicationUser = await _context.ApplicationUsers.FindAsync(AuthenticatedUserId());

            // Find ChatChannel - Considera os canais disponívis para o usuário autenticado - considerando o setor e o usuario do canal, se houver especificados
            ChatChannelDto ChatChannelDto = await _context.ChatChannels
                .Where(p => p.GroupId == AuthorizedGroupId() &
                (
                ((p.ApplicationUserId == null
                 &
                 (p.DepartmentId == null | p.DepartmentId == applicationUser.DepartmentId)) | p.ApplicationUserId == AuthenticatedUserId())
                 |
                 AuthenticatedUserRole() == "groupadmin")
                )
                .Select(p => new ChatChannelDto(p))
                .FirstOrDefaultAsync();

            // If not found
            if (ChatChannelDto == null)
            {
                return NotFound($"Não foi encontrado o ChatChannel com Id: {id}");
            }
            else
            {
                return ChatChannelDto;
            }
        }

        // POST api/<ChatChannelsController>
        [HttpPost]
        public async Task<ActionResult<ChatChannelDto>> PostChatChannel(ChatChannel chatChannel)
        {

            // Se informou AppService name, confere se é unico na base
            if ( !string.IsNullOrEmpty(chatChannel.AppName))
            {
                // Check if ChatChannel Id existe at database
                ChatChannel oldChatChannel = await _context.ChatChannels
                                    .Where(p => p.AppName == chatChannel.AppName)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                // Se já tem algum outro canal com mesmo AppName
                if (oldChatChannel != null)
                {
                    return BadRequest($"Já existe na base outro canal com este mesmo AppName: {chatChannel.AppName}");
                }
            }

            // Bind Group
            chatChannel.GroupId = AuthorizedGroupId();

            // SaveGroupMessages: default=true
            chatChannel.SaveGroupMessages = true;

            // EnableTextToSpeech: default=false
            chatChannel.EnableTextToSpeech = false;

            // Add 
            _context.ChatChannels.Add(chatChannel);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ChatChannelExists(chatChannel.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("PostChatChannel", new { id = chatChannel.Id }, new ChatChannelDto(chatChannel));
        }

        // PUT api/<ChatChannelsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatChannel(string id, ChatChannel chatChannel)
        {

            // Se informou AppService name, confere se é unico na base
            if (!string.IsNullOrEmpty(chatChannel.AppName))
            {
                // Check if ChatChannel Id existe at database
                ChatChannel otherChatChannel = await _context.ChatChannels
                                    .Where(p => p.AppName == chatChannel.AppName)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                // Se já tem algum outro canal com mesmo AppName
                if (otherChatChannel != null)
                {
                    return BadRequest($"Já existe na base outro canal com este mesmo AppName: {chatChannel.AppName}");
                }
            }

            // Check if ChatChannel has Id
            if (id != chatChannel.Id | id==string.Empty)
            {
                string error = "Não foi informado um ChatChannel válido com seu ID.";
                return BadRequest(error);
            }

            // Check if ChatChannel Id existe at database
            ChatChannel oldChatChannel = await _context.ChatChannels
                                .Where(p=> p.Id == id)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (oldChatChannel == null)
            {
                string error = "ChatChannel {id} não localizado na base.";
                return NotFound(error);
            }

            // Bind Group
            chatChannel.GroupId = AuthorizedGroupId();

            // Update Database
            _context.Update(chatChannel);

            // Return
            return NoContent();
        }

        // DELETE api/<ChatChannelsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ChatChannelDto>> Delete(string id)
        {
            var ChatChannel = await _context.ChatChannels
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (ChatChannel == null)
            {
                return NotFound();
            }

            _context.ChatChannels.Remove(ChatChannel);
            await _context.SaveChangesAsync();

            return new ChatChannelDto(ChatChannel);


        }

        private bool ChatChannelExists(string id)
        {
            return _context.ChatChannels.Any(e => e.Id == id);
        }

    }
}
