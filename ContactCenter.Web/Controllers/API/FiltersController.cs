using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FiltersController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FiltersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Filters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FilterDto>>> GetFilters(int boardId)
        {
            if ( boardId==0)
                return await _context.Filters
                    .Where(p =>p.GroupId == AuthorizedGroupId() & (p.ApplicationUserId == AuthenticatedUserId() | p.ApplicationUserId==null))
                    .Select(f => new FilterDto(f))
                    .ToListAsync();
            else
                return await _context.Filters
                    .Where(p => p.GroupId == AuthorizedGroupId() & (p.ApplicationUserId == AuthenticatedUserId() | p.ApplicationUserId == null) & p.BoardId==boardId)
                    .Select(f => new FilterDto(f))
                    .ToListAsync();
        }

        // GET: api/Filters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FilterDto>> GetFilter(int id)
        {
            var filter = await _context.Filters
                        .Where(p => p.Id == id)
                        .FirstOrDefaultAsync();

            if (filter == null)
            {
                return NotFound();
            }
            else if ( filter.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            return new FilterDto(filter);
        }

        // PUT: api/Filters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutField(int id, FilterDto filterDto)
        {
            if (id != filterDto.Id)
            {
                return BadRequest();
            }

            // Check if Id exist and belongs to Authorized GroupId
            Filter filter = await _context.Filters
                .FindAsync(id);

            if (filter == null)
            {
                return NotFound();
            }
            else if (filter.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            filter.Title = filterDto.Title;
            filter.JsonFilter = filterDto.JsonFilter;

            _context.Update(filter);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilterExists(id))
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

        // POST: api/Filters
        [HttpPost]
        public async Task<ActionResult<FilterDto>> PostFilter(Filter filter)
        {

            // GroupId binding
            filter.GroupId = AuthorizedGroupId();

            // ApplicationUserId binding
            filter.ApplicationUserId = AuthenticatedUserId();

            _context.Filters.Add(filter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostFilter", new { id = filter.Id }, new FilterDto(filter));
        }

        // DELETE: api/Filters/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FilterDto>> DeleteFilter(int id)
        {
            // Confere se o Field existe
            var filter = await _context.Filters.FindAsync(id);
            if (filter == null)
            {
                return NotFound($"Não foi encontrado este filtro:{id}");
            }

            // Confere se pertence ao grupo
            else if ( filter.GroupId != AuthorizedGroupId())
            {
                return Unauthorized("Você não tem permissão para excluir este filtro, pois não pertence ao seu grupo.");
            }

            try
			{
                _context.Filters.Remove(filter);
                await _context.SaveChangesAsync();
            }
            catch ( Exception ex )
            {
                string msg;
                if (ex.InnerException != null)
                    msg = ex.InnerException.Message;
                else
                    msg = ex.Message;

                if (msg.Contains("FK_Sendings_Filters_FilterId"))
				{
                    return BadRequest("Não é possível excluir este filtro, pois há um Envio vinculado ao mesmo.");
				}
				else
				{
                    return BadRequest(msg);
				}
            }

            return new FilterDto(filter);
        }

        private bool FilterExists(int id)
        {
            return _context.Filters.Any(e => e.Id == id);
        }
    }
}
