using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ContactCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BoardsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;


        public BoardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards(string ApplicationUserId)
        {

            if (string.IsNullOrEmpty(ApplicationUserId))
            {
                return await _context.Boards
                    .Where(p => p.GroupId == AuthorizedGroupId())
                    .Include( s=>s.Stages)
                    .Include( bf=>bf.BoardFields)
                    .ThenInclude ( f=>f.Field)
                    .ThenInclude ( d=>d.DataListValues)
                    .Select(b => new BoardDto(b))
                    .ToListAsync();

            }
            else
            {
                ApplicationUser applicationUser = _context.ApplicationUsers
                                                .Where(p => p.Id == ApplicationUserId)
                                                .FirstOrDefault();
                if (applicationUser == null)
                {
                    return NotFound();
                }
                else
                {
                    return await _context.Boards
                    .Where(p => p.ApplicationUserId == AuthenticatedUserId() | (p.GroupId == AuthorizedGroupId() & p.ApplicationUserId == null & (p.DepartmentId == applicationUser.DepartmentId | p.DepartmentId == 0)))
                    .Include(s => s.Stages)
                    .Include(bf => bf.BoardFields)
                    .ThenInclude(f => f.Field)
                    .ThenInclude(d => d.DataListValues)
                    .Select(b => new BoardDto(b))
                   .ToListAsync();
                }
            }
        }
        // GET: api/Boards/currentUser
        [HttpGet("CurrentUser")]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoardsFromCurrentUser()
        {

            ApplicationUser applicationUser = _context.ApplicationUsers
                                            .Where(p => p.Id == AuthenticatedUserId())
                                            .FirstOrDefault();
            if ( applicationUser == null)
            {
                return NotFound();
            }
            else
            {
                return await _context.Boards
                    .Where(p => p.GroupId == AuthorizedGroupId() &
                            (
                                AuthenticatedUserRole() == "groupadmin"
                                | p.ApplicationUserId == AuthenticatedUserId()
                                | ( p.DepartmentId == applicationUser.DepartmentId & p.ApplicationUserId==null)
                            )
                    )
                    .Include(s => s.Stages)
                    .Include(bf => bf.BoardFields)
                    .ThenInclude(f => f.Field)
                    .ThenInclude(d => d.DataListValues)
                    .Select(b => new BoardDto(b))
                    .ToListAsync();
            }

        }
        // GET: api/Boards/5

        [HttpGet("{id}")]
        public async Task<ActionResult<BoardDto>> GetBoard(int id)
        {
            var board = await _context.Boards
                            .Where(p=>p.Id == id)
                            .Include(s=>s.Stages)
                            .FirstOrDefaultAsync();

            board.Stages = board.Stages.OrderBy(p => p.Order).ToList();

            if (board == null)
                return NotFound();

            else if (AuthorizedGroupId() != board.GroupId)
                return Unauthorized();


            // Creates a BoardDTO based on this board
            BoardDto boardDto = new BoardDto(board);

            // Get all fields definition
            // then left join Board Fields - fields definition saved to this board
            var boardFields = from a in _context.Fields
                              join b in _context.BoardFields.Where(p=>p.BoardId == id)
                              on a.Id equals b.FieldId into ab
                              from c in ab.DefaultIfEmpty()
                              orderby c.Order
                              select new BoardField { BoardId=id, Enabled= c==null ? false : c.Enabled, Field=a, FieldId=a.Id, Id = c == null ? 0 : c.Id, Order = c==null ? 0 : c.Order };

            // Feed our Board with the collection of all fields definition, joined with the fields definition already saved to this board
            foreach ( BoardField boardField in boardFields)
            {
                // Add BoardField to Board
                boardDto.AddBoardField(boardField);
            }

            return boardDto;
        }

        // PUT: api/Boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBoard(int id, Board board)
        {
            if (id != board.Id)
            {
                return BadRequest();
            }

            // Check if Id exist and belongs to Authorized GroupId
            var oldBoard = await _context.Boards.FindAsync(id);
            if (oldBoard == null)
            {
                return NotFound();
            }
            else if (oldBoard.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            // Set GroupID
            board.GroupId = AuthorizedGroupId();

			// We need to clear boardFiels.Field descriptor, otherwise it will insert duplicated Field
			foreach (BoardField boardField in board.BoardFields)
			{
				boardField.Field = null;
			}

			// Then we can save, but we cannot save the object we get at the put, because it will raise an error,
			// as the PrimaryKey is beeing tracked with the object we needed to create to check if Id exists at database, and belongs to authorized group.
			// So, we will copy values from the receaved object to the object we used to search the database.
			oldBoard.CopyFrom(board);
            _context.Update(oldBoard);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoardExists(id))
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

        // POST: api/Boards
        [HttpPost]
        public async Task<ActionResult<BoardDto>> PostBoard(Board board)
        {
            // Remove Id for insert a new record
            board.Id = 0;

            // Check GroupId
            board.GroupId = AuthorizedGroupId();

            // Check if user is not a GroupAdmin
            if (AuthenticatedUserRole() != "groupadmin")
                // If user is not a groupAdmin, new board must be bound to the user
                board.ApplicationUserId = AuthenticatedUserId();

            // Add new board;
            _context.Boards.Add(board);

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetBoard", new { id = board.Id }, new BoardDto(board));

            }
            catch ( Exception ex)
            {
                throw (ex);
            }

        }

        // DELETE: api/Boards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BoardDto>> DeleteBoard(int id)
        {
            // Confere se o Board existe
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
                return NotFound();

            // Confere se o Board pertence ao grupo do usuário logado
            if (AuthorizedGroupId() != board.GroupId)
                return Unauthorized($"Você não tem permissão para excluir este quadro.");

            // Confer se o Board pertence ao usuário logado, ou se o usuario é administrador do grupo
            if (board.ApplicationUserId != AuthenticatedUserId() & AuthenticatedUserRole() != "groupadmin")
                return Unauthorized($"Você não tem permissão para excluir este quadro.");

            // Confere se este Board ( lista ) tem estágios
            var Stages = await _context.Stages
                        .Where(p => p.BoardId == id)
                        .ToListAsync();

            try
            {
                // Para todos os estágios
                foreach (Stage stage in Stages)
                {
                    // Exclui o estágio
                    _context.Stages.Remove(stage);
                }

                // Confere se tem filtros
                if (board.Filters != null)
                {
                    foreach (Filter filter in board.Filters)
                    {
                        // Exclui o filtro 
                        _context.Filters.Remove(filter);
                    }
                }

                // Exclui o Board ( Lista )
                _context.Boards.Remove(board);

                // Commit to database
                await _context.SaveChangesAsync();

                // Retorna
                return Ok();
            }

            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("GroupCampaign"))
                    return BadRequest("Esta lista não pode ser excluída, pois há uma Campanha de Links de Grupos que depende dela.");
                else
                    return BadRequest(ex.Message);
            }

        }

        private bool BoardExists(int id)
        {
            return _context.Boards.Any(e => e.Id == id);
        }

    }
}
