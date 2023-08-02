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
    public class FieldsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FieldsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Fields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FieldDto>>> GetFields()
        {
            return await _context.Fields
                .Where(p => p.IsGlobal || p.GroupId == AuthorizedGroupId())
                .Include(q=>q.DataListValues)
                .OrderBy(o => o.IsGlobal)
                .Select(f => new FieldDto(f))
                .ToListAsync();
        }

        // GET: api/Fields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FieldDto>> GetField(int id)
        {
            var @field = await _context.Fields
                        .Where(p => p.Id == id)
                        .Include(q => q.DataListValues)
                        .FirstOrDefaultAsync();

            if (@field == null)
            {
                return NotFound();
            }
            else if ( !field.IsGlobal && field.GroupId != null && field.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            return new FieldDto(@field);
        }

        // PUT: api/Fields/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutField(int id, FieldDto fieldDto)
        {
            if (id != fieldDto.Id)
            {
                return BadRequest();
            }

            // Check if Id exist and belongs to Authorized GroupId
            Field field = await _context.Fields
                .Where(p=>p.Id==id && p.GroupId == AuthorizedGroupId())
                .Include( p=>p.DataListValues)
                .FirstOrDefaultAsync();
            if (field == null)
            {
                return NotFound();
            }

            // Se for campo do tipo DataList
            if ( field.FieldType == FieldType.DataList && field.DataListValues != null)
            {
                // Exclui data list values ( porque vai inserir os novos )
                foreach (DataListValue dataListValue in field.DataListValues)
                    _context.DataListValues.Remove(dataListValue);
                await _context.SaveChangesAsync();
                field.DataListValues = null;
            }

            field.CopyFromDto(fieldDto);
            _context.Update(field);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FieldExists(id))
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

        // POST: api/Fields
        [HttpPost]
        public async Task<ActionResult<FieldDto>> PostField(Field @field)
        {

            // GroupId binding
            field.GroupId = AuthorizedGroupId();
            field.IsGlobal = false;

            _context.Fields.Add(@field);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetField", new { id = @field.Id }, new FieldDto(@field));
        }

        // DELETE: api/Fields/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FieldDto>> DeleteField(int id)
        {
            // Confere se o Field existe
            var @field = await _context.Fields.FindAsync(id);
            if (@field == null)
            {
                return NotFound($"Não foi encontrado o campo:{id}");
            }

            // Confere se é global
            else if (field.IsGlobal)
            {
                return Unauthorized($"Você não tem permissão para excluir este campo: {@field.Label}, pois é um campo Global.");
            }

            // Confere se pertence ao grupo
            else if ( field.GroupId != AuthorizedGroupId())
            {
                return Unauthorized($"Você não tem permissão para excluir este campo: {@field.Label}, pois não pertence ao seu grupo.");
            }

            // Confer se o o usuario é administrador do grupo
            if (AuthenticatedUserRole() != "groupadmin")
			{
                return Unauthorized($"Você não tem permissão para excluir este campo: {id}.");
            }

            _context.Fields.Remove(@field);
            await _context.SaveChangesAsync();

            return new FieldDto(@field);
        }

        private bool FieldExists(int id)
        {
            return _context.Fields.Any(e => e.Id == id);
        }
    }
}
