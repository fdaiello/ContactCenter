using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using Microsoft.Extensions.Configuration;
using ContactCenter.Infrastructure.Utilities;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StagesController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public StagesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Stages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StageDto>>> GetStages(int boardId)
        {

            if ( boardId >0 )
                // Get all stages from given Board
                return await _context.Stages
                            .Where(p => p.BoardId == boardId && p.Board.GroupId == AuthorizedGroupId())
                            .Include(b => b.Board)
                            .Select(q=> new StageDto(q))
                            .ToListAsync();
            else
                // Get all stages from authenticated group
                return await _context.Stages
                            .Where(p => p.Board.GroupId == AuthorizedGroupId())
                            .Include(b => b.Board)
                            .Select(q => new StageDto(q))
                            .ToListAsync();
        }

        // GET: api/Stages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StageDto>> GetStage(int id)
        {
            Stage stage = await _context.Stages
					.Where(p => p.Id == id)
					.Include(b => b.Board)
					.Where(b1 => b1.Board.GroupId == AuthorizedGroupId())
					.FirstOrDefaultAsync();

            if (stage == null)
                return NotFound($"Estagio {id} não existe.");

            List<Card> cards = await _context.Cards
                    .Where(p => p.StageId == id)
                    .Include( cf => cf.CardFieldValues)
                    .ThenInclude ( f=> f.Field)
                    .Include(c => c.Contact)
                    .Where(p => p.Contact.ApplicationUserId == null || p.Contact.ApplicationUserId == AuthenticatedUserId() || AuthenticatedUserRole() == "groupadmin" || AuthenticatedUserRole() == "supervisor" )
                    .Take(stage.ShowMax==0?99:stage.ShowMax)
                    .ToListAsync(); 

            stage.Cards = cards;

            // Se tem cartões
            if ( stage.Cards != null)
			{
                // Adiciona o caminho no avatar
                foreach (Card card in stage.Cards)
				{
                    card.Contact.PictureFileName = string.IsNullOrEmpty(card.Contact.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), card.Contact.PictureFileName);
                }

            }

            StageDto stageDto = new StageDto(stage);
            return stageDto;
        }

        // PUT: api/Stages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStage(int id, Stage stage)
        {
            if (id != stage.Id)
            {
                return BadRequest();
            }

            // Check if Board belongs to the Authorized GroupId
            var board = await _context.Boards.FindAsync(stage.BoardId);
            if (board == null || board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            // Se o estágio tem cartões
            if ( stage.Cards != null)
            {
                // We have to clean CONTACT descriptor. As contact can appear in several cards, it would cause an error
                foreach (Card card in stage.Cards)
                {
                    card.Contact = null;

                    // Vamos limpar também a definição dos campos, para não fazer alteranções neles
                    foreach (CardFieldValue cardFieldValue in card.CardFieldValues)
					{
                        cardFieldValue.Field = null;
					}

                }
            }
            

            // Update
            _context.Update(stage);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Stages
        [HttpPost]
        public async Task<ActionResult<StageDto>> PostStage(Stage stage)
        {

            // Check if Board belongs to the Authorized GroupId
            var board = await _context.Boards.FindAsync(stage.BoardId);
            if (board == null )
            {
                return NotFound();
            }
            else if ( board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            _context.Stages.Add(stage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStage", new { id = stage.Id }, new StageDto(stage));

        }

        // DELETE: api/Stages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<StageDto>> DeleteStage(int id)
        {

            // Confere se o Stage existe
            Stage stage = await _context.Stages.Where(p => p.Id == id).Include(b => b.Board).FirstOrDefaultAsync();
            if (stage == null)
            {
                return NotFound($"Não foi encontrado o estágio: {id}");
            }
            // Confere se o Stage pertence ao grupo
            else if ( stage.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized($"Você não tem permissão para excluir este estágio: {id}");
            }

            // Confere se o Board deste Stage pertence ao usuário logado, ou se o usuario é administrador do grupo
            if (stage.Board.ApplicationUserId != AuthenticatedUserId() & AuthenticatedUserRole() != "groupadmin")
                return Unauthorized($"Você não tem permissão para excluir este estágio.");

            // Confere se tem cartões dentro do estágio
            var cards = await _context.Cards
                        .Where(p => p.StageId == id)
                        .ToListAsync();

            if (cards.Any())
            {
                return Unauthorized($"Este estágio não pode ser excluido porque tem cartões dentro dele. Mova os cartões para outro estágio primeiro.");
            }

            // Exclui
            _context.Stages.Remove(stage);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool StageExists(int id)
        {
            return _context.Stages.Any(e => e.Id == id);
        }
    }
}
