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
    public class ContactFieldsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactFieldsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ContactFields/JoinFields
        [HttpGet("JoinFields")]
        public async Task<ActionResult<IEnumerable<ContactFieldDto>>> GetContactFieldsJoin()
        {
            // Get all FIELDS
            // then left join Contact Fields - fields definition saved to contacts
            var query = from a in _context.Fields.Where(p=>p.IsGlobal || p.GroupId == AuthorizedGroupId())
                                   join b in _context.ContactFields.Where(p=>p.GroupId == AuthorizedGroupId())
                                   on a.Id equals b.FieldId into ab
                                   from c in ab.DefaultIfEmpty()
                                   orderby c.Order
                                   select new ContactField { Enabled = c.Enabled, FieldId = a.Id, Id = c.Id, Order = c.Order, Field=a };

            List<ContactField> contactFields = await query.ToListAsync();

            // Convert list of contactFiels to list of contactFieldsDto
            List<ContactFieldDto> contactFieldDtos = new List<ContactFieldDto>();
            foreach ( ContactField contactField in contactFields)
            {
                contactFieldDtos.Add(new ContactFieldDto(contactField));
            }
            return contactFieldDtos;
        }
        // GET: api/ContactFields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactFieldDto>>> GetContactFields()
        {
            return await _context.ContactFields
                        .Where(p => p.GroupId == AuthorizedGroupId())
                        .Include(f => f.Field)
                        .ThenInclude(d => d.DataListValues)
                        .OrderBy(o => o.Order)
                        .Select(q => new ContactFieldDto(q))
                        .ToListAsync();
        }

        // GET: api/ContactFields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactFieldDto>> GetContactField(int id)
        {
            var contactField = await _context.ContactFields
                                    .Where(p => p.Id == id)
                                    .Include(f => f.Field)
                                    .ThenInclude(d => d.DataListValues)
                                    .FirstOrDefaultAsync();

            if (contactField == null)
            {
                return NotFound();
            }
            else if ( contactField.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            return new ContactFieldDto(contactField);
        }

        // PUT: api/ContactFields/JoinFields
        [HttpPut("JoinFields")]
        public async Task<IActionResult> PutContactFieldJoin(List<ContactField> contactFields)
        {

            // Varre todos a lista recebida
            foreach ( ContactField contactField in contactFields)
            {
                // valida se o FieldID existe, e pertence ao grupo o é global
                var field = await _context.Fields.FindAsync(contactField.Field.Id);
                if (field == null)
                {
                    string error = $"Campo não localizado no banco. FieldId:{field.Id}; Field.Label: {field.Label}.";
                    return BadRequest(error);
                }
                else if (!field.IsGlobal & field.GroupId != AuthorizedGroupId())
                {
                    string error = $"Json contém um campo que não é Global e não pertence ao grupo atual. FieldId:{field.Id}; Field.Label: {field.Label}.";
                    return BadRequest(error);
                }

                // Limpa o descritor do campo Field
                contactField.Field = null;

                // Bind GroupID
                contactField.GroupId = AuthorizedGroupId();

                // confere se o Id do Contact Field é 0 - o registro não existe
                if (contactField.Id == 0)
                {
                    // Confere se o ID deste campo já está salvo
                    var contactField0 = _context.ContactFields
                                        .Where(p => p.FieldId == contactField.FieldId & p.GroupId == AuthorizedGroupId())
                                        .FirstOrDefault();

                    // Se achou registro salvo
                    if (contactField0 != null)
                    {
                        // Atualiza
                        contactField0.Enabled = contactField.Enabled;
                        contactField0.Order = contactField.Order;
                        _context.ContactFields.Update(contactField0);
                    }
                    else
                    {
                        // Adiciona o objeto
                        _context.ContactFields.Add(contactField);
                    }
                }
                else
                {
                    // Altera
                    _context.ContactFields.Update(contactField);
                }

            }

            // Salva no banco
            await _context.SaveChangesAsync();

            // Return
            return Ok();
        }

        // PUT: api/ContactFields/Sort
        [HttpPut("Sort")]
        public async Task<IActionResult> SortContactFields(int movedId, int overIndex)
        {

            // Busca os ContactFields
            List<ContactField> contactFields = await _context.ContactFields
                                                .Where(p => p.GroupId == AuthorizedGroupId())
                                                .OrderBy(o => o.Order)
                                                .ToListAsync();

            // Varre e reordena
            int sortIndex = 0;
            foreach ( ContactField contactField in contactFields)
            {
                // Se o sortIndex está na posição para o qual a linha foi movida, vamos pular
                if (sortIndex == overIndex)
                    sortIndex++;

                // Quando achar o elemento que foi movido
                if (contactField.Id == movedId)
                    // A ordem dele é a posição para a qual ele foi movido na Grid
                    contactField.Order = overIndex;
                else
                {
                    // Para os demais, segue o baile na ordenação normal
                    contactField.Order = sortIndex;

                    // incrementa o indice
                    sortIndex++;
                }

                // Altera
                _context.ContactFields.Update(contactField);
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

        // PUT: api/ContactFields/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContactField(int id, ContactField contactField)
        {
            if (id != contactField.Id)
            {
                string error = $"ContactField.Id na URL {id} diferente do ContactField.ID dentro do Json {contactField.Id}.";
                return BadRequest(error);
            }

            // Check if Field exists and belongs to the Authorized GroupId
            var field = await _context.Fields.FindAsync(contactField.FieldId);
            if (field == null)
            {
                string error = $"Field não encontrado. Field.Id: {field.Id}.";
                return BadRequest(error);
            }
            else if ( !field.IsGlobal & field.GroupId != AuthorizedGroupId())
            {
                string error = $"Json contém campo que não é Global ou pertence a outro grupo. Field.Id: {field.Id}.";
                return BadRequest(error);
            }

            // Bind GroupId
            contactField.GroupId = AuthorizedGroupId();

            _context.Entry(contactField).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactFieldExists(id))
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

        // POST: api/ContactFields
        [HttpPost]
        public async Task<ActionResult<ContactFieldDto>> PostContactField(ContactField contactField)
        {

            if (contactField.Id!= 0)
            {
                string error = $"Para inserir novo ContactField, o ID deve ser zero.";
                return BadRequest(error);
            }

            // Contact Field to Field : One to One Relation; cannot have multiples
            var hasContactField = _context.ContactFields.Where(p => p.FieldId == contactField.FieldId).FirstOrDefault();
            if ( hasContactField != null)
            {
                string error = $"já existe com objeto ContactField salvo no banco com este mesmo FieldId: {contactField.FieldId}.";
                return BadRequest(error);
            }

            // If fieldID is set
            if ( contactField.FieldId != 0)
            {
                // Check if Field exists and belongs to the Authorized GroupId
                var field = await _context.Fields.FindAsync(contactField.FieldId);
                if (field == null)
                {
                    string error = $"Json contém campo que não foi encontrado no banco. Field.Id: {field.Id}.";
                    return BadRequest(error);
                }
                else if (!field.IsGlobal & field.GroupId != AuthorizedGroupId())
                {
                    string error = $"Json contém campo que não é Global ou pertence a outro grupo. Field.Id: {field.Id}.";
                    return BadRequest(error);
                }

                // Clear Field descriptor
                contactField.Field = null;
            }
            // If fieldId == 0 - will insert a new field
            else
            {
                // Then we need to bind Field.GroupID
                contactField.Field.GroupId = AuthorizedGroupId();
                contactField.Field.IsGlobal = false;
            }

            // Bind GroupId
            contactField.GroupId = AuthorizedGroupId();

            // And Add to DB
            _context.ContactFields.Add(contactField);
            await _context.SaveChangesAsync();

            return CreatedAtAction("ContactField", new { id = contactField.Id }, new ContactFieldDto(contactField));
        }

        // DELETE: api/ContactFields/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ContactFieldDto>> DeleteContactField(int id)
        {
            var contactField = await _context.ContactFields.FindAsync(id);

            if (contactField == null)
            {
                return NotFound();
            }
            else if (contactField.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            _context.ContactFields.Remove(contactField);
            await _context.SaveChangesAsync();

            return new ContactFieldDto(contactField);
        }

        private bool ContactFieldExists(int id)
        {
            return _context.ContactFields.Any(e => e.Id == id);
        }
    }
}