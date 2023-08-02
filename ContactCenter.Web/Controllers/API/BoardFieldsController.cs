using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using System.Net.Http;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BoardFieldsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BoardFieldsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BoardFields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardFieldDto>>> GetBoardFields(int boardId)
        {
            // If boardID was set
            if (boardId > 0)
            {
                // Check if Board Exist
                Board board = await _context.Boards.FindAsync(boardId);
                if (board == null)
                    return NotFound();

                // Get all fields definition
                // then left join Board Fields - fields definition saved to this board
                // At last, convert BoardField to BoardFieldDto
                var boardFieldsDto = from a in _context.Fields.Where(f=>f.IsGlobal | f.GroupId == AuthorizedGroupId())
                                  join b in _context.BoardFields.Where(p => p.BoardId == boardId)
                                  on a.Id equals b.FieldId into ab
                                  from c in ab.DefaultIfEmpty()
                                  orderby c.Order
                                  select new BoardFieldDto (new BoardField { BoardId = boardId, Enabled = c.Enabled, Field = a, FieldId = a.Id, Id = c.Id, Order = c.Order });

                return await boardFieldsDto.ToListAsync();
            }
            // If boardID was not set
            else
            {
                // Return existing BoardFields for all Boards
                return await _context.BoardFields
                         .Where(p => p.Board.GroupId == AuthorizedGroupId())
                         .Include(f => f.Field)
                         .OrderBy(o => o.Order)
                         .Select(q => new BoardFieldDto(q))
                         .ToListAsync();
            }
        }

        // GET: api/BoardFields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardFieldDto>> GetBoardField(int id)
        {
            var BoardField = await _context.BoardFields
                                    .Where(p => p.Id == id)
                                    .Include(f => f.Field)
                                    .Include(b=>b.Board)
                                    .FirstOrDefaultAsync();

            if (BoardField == null)
            {
                return NotFound();
            }
            else if ( BoardField.Board !=null && BoardField.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            return new BoardFieldDto(BoardField);
        }

        // PUT: api/BoardFields/boardId=<boardId>
        [HttpPut]
        public async Task<IActionResult> PutBoardField(int boardId, List<BoardField> boardFields)
        {
            // Check if Board Exist
            Board board = await _context.Boards.FindAsync(boardId);
            if (board == null)
                return NotFound();

            // para cada elemento da lista
            foreach ( BoardField boardField in boardFields)
			{
                // valida se o BoardId confere
                if (boardField.BoardId != boardId)
                    return BadRequest();

                // valida se o FieldID existe, e pertence ao grupo o é global
                var field = await _context.Fields.FindAsync(boardField.Field.Id);
                if (field == null)
                    return NotFound();
                else if (!field.IsGlobal & field.GroupId != AuthorizedGroupId())
                    return Unauthorized();

                // Limpa o descritor do campo Field
                boardField.Field = null;

                // confere se o Id do BoardField é 0 - indica que ainda não foi salvo para este Board - ou não atualizou no client
                if ( boardField.Id == 0)
				{
                    BoardField boardField1 = await _context.BoardFields
                                            .Where(p => p.BoardId == boardField.BoardId && p.FieldId == boardField.FieldId)
                                            .FirstOrDefaultAsync();

                    if ( boardField1 !=null)
                    {
                        // Altera
                        boardField1.Enabled = boardField.Enabled;
                        _context.BoardFields.Update(boardField1);

                        // Insere em todos os cartões desta lista o CardFieldValue correspondente
                        await InsertCardFieldValues(boardField1);
                    }
                    else
                    {
                        // Adiciona o objeto
                        _context.BoardFields.Add(boardField);

                        // Insere em todos os cartões desta lista o CardFieldValue correspondente
                        await InsertCardFieldValues(boardField);
                    }

                }
                else
				{
                    // Altera
                    _context.BoardFields.Update(boardField);

                    // Se ativou
                    if (boardField.Enabled)
                    {
                        // Insere em todos os cartões desta lista o CardFieldValue correspondente
                        await InsertCardFieldValues(boardField);
                    }
                    else
                    {
                        // Remove de todos os cartões desta lista o CardFieldValue correspondente
                        await RemoveCardFieldValues(boardField);
                    }
                }
			}

            // Salva no banco
            await _context.SaveChangesAsync();

            // Return
            return Ok();
        }
        // Insere em todos os cartões desta lista o CardFieldValue correspondente
        private async Task InsertCardFieldValues(BoardField boardField)
        {
            // Varre todos os cartoes desta lista
            List<Card> cards = await _context.Cards
                                .Where(p => p.Stage.BoardId == boardField.BoardId)
                                .Include(p=>p.CardFieldValues)
                                .ToListAsync();

            foreach ( Card card in cards)
            {
                // se ainda nao tem o CardFieldValue do field que foi marcado
                if ( !card.CardFieldValues.Where(p => p.FieldId == boardField.FieldId).Any())
                {
                    CardFieldValue cardFieldValue = new CardFieldValue { CardId = card.Id, FieldId = boardField.FieldId };
                    _context.CardFieldValues.Add(cardFieldValue);
                }
            }

            await _context.SaveChangesAsync();
        }
        // Remove de todos os cartões desta lista o CardFieldValue correspondente
        private async Task RemoveCardFieldValues(BoardField boardField)
        {
            // Varre todos os cartoes desta lista
            List<Card> cards = await _context.Cards
                                .Where(p => p.Stage.BoardId == boardField.BoardId)
                                .Include(p => p.CardFieldValues)
                                .ToListAsync();

            foreach (Card card in cards)
            {
                foreach ( CardFieldValue cardFieldValue in card.CardFieldValues.Where(p => p.FieldId == boardField.FieldId))
                {
                    _context.CardFieldValues.Remove(cardFieldValue);
                }
            }

            await _context.SaveChangesAsync();
        }


        // Confere se podemos alterar um BoardField
        // Não permite desmarcar caso o boardField já esteja sendo usado
        private async Task<bool> CanUpdateBoardField ( BoardField boardField)
		{
            // sempre permite marcar
            if ( boardField.Enabled)
			{
                return true;
			}
            // se esta sendo desmarcado
            else
			{
                // verifica se tem CardFieldValues usando este BoardField ( usando este field neste board )
                var cardFieldValues = await _context.CardFieldValues
                                .Where(p => p.FieldId == boardField.FieldId && p.Card.Stage.BoardId == boardField.BoardId)
                                .ToListAsync();

                if ( cardFieldValues.Count() == 0 )
				{
                    return true;
				}
                else
				{
                    return false;
				}
			}
		}

        // PUT: api/BoardFields/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBoardField(int id, BoardField boardField)
        {
            if (id != boardField.Id)
            {
                return BadRequest();
            }

            // If fieldID is set
            if (boardField.FieldId != 0)
            {
                // Check if Field exists and belongs to the Authorized GroupId
                var field = await _context.Fields.FindAsync(boardField.FieldId);
                if (field == null)
                {
                    return NotFound();
                }
                else if (!field.IsGlobal & field.GroupId != AuthorizedGroupId())
                {
                    return Unauthorized();
                }

            }
            else
            {
                // Then we need to bind Field.GroupID
                boardField.Field.GroupId = AuthorizedGroupId();
                boardField.Field.IsGlobal = false;
            }


            _context.Entry(boardField).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoardFieldExists(id))
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

        // PUT: api/BoardFields/Sort
        [HttpPut("Sort")]
        public async Task<IActionResult> SortBoardFields(int boardId, int movedId, int overIndex)
        {
            // Busca os BoardFields
            List<BoardField> boardFields = await _context.BoardFields
                                                .Where(p => p.Board.GroupId == AuthorizedGroupId() & p.Board.Id == boardId)
                                                .OrderBy(o => o.Order)
                                                .ToListAsync();

            if (boardFields.Count == 0)
                return NotFound($"Nenhum BoardField encontrado para o quadro {boardId}!");

            // Varre e reordena
            int sortIndex = 0;
            foreach (BoardField boardField in boardFields)
            {
                // Se o sortIndex está na posição para o qual a linha foi movida, vamos pular
                if (sortIndex == overIndex)
                    sortIndex++;

                // Quando achar o elemento que foi movido
                if (boardField.Id == movedId)
                    // A ordem dele é a posição para a qual ele foi movido na Grid
                    boardField.Order = overIndex;
                else
                {
                    // Para os demais, segue o baile na ordenação normal
                    boardField.Order = sortIndex;

                    // incrementa o indice
                    sortIndex++;
                }

                // Altera
                _context.BoardFields.Update(boardField);
            }

            try
            {
                // Salva no banco
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            // Return
            return Ok();
        }

        // POST: api/BoardFields
        [HttpPost]
        public async Task<ActionResult<BoardFieldDto>> PostBoardField(BoardField boardField)
        {

            if (boardField.Id!= 0)
            {
                return BadRequest();
            }

            // Check if its using an existing field
            if ( boardField.Field.Id != 0)
            {
                // BoardField to Field : One to One Relation; cannot have multiples
                var hasBoardField = _context.BoardFields.Where(p => p.BoardId == boardField.BoardId & p.FieldId == boardField.FieldId).FirstOrDefault();
                if (hasBoardField != null)
                {
                    return Content("Error: field already configured to this Board. Cannot duplicate.");
                }

                // Check if Field exists and belongs to the Authorized GroupId
                var field = await _context.Fields.FindAsync(boardField.FieldId);
                if (field == null)
                {
                    return NotFound();
                }
                else if (!field.IsGlobal & field.GroupId != AuthorizedGroupId())
                {
                    return Unauthorized();
                }

                // Clear Field descriptor
                boardField.Field = null;

            }
            // If its creating a new field
            {
                // Bind field to Group
                boardField.Field.GroupId = AuthorizedGroupId();
                // And mark it as not Global
                boardField.Field.IsGlobal = false;
            }

            _context.BoardFields.Add(boardField);
            await _context.SaveChangesAsync();

            return CreatedAtAction("BoardField", new { id = boardField.Id }, new BoardFieldDto(boardField));
        }

        // DELETE: api/BoardFields/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BoardFieldDto>> DeleteBoardField(int id)
        {
            var BoardField = await _context.BoardFields
                                .Where(p=>p.Id == id)
                                .Include(b=>b.Board)
                                .FirstOrDefaultAsync();

            if (BoardField == null)
            {
                return NotFound();
            }
            else if (BoardField.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            _context.BoardFields.Remove(BoardField);
            await _context.SaveChangesAsync();

            return new BoardFieldDto(BoardField);
        }

        private bool BoardFieldExists(int id)
        {
            return _context.BoardFields.Any(e => e.Id == id);
        }
    }
}
