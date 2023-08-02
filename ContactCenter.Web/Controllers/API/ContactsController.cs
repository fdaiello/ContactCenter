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
using ContactCenter.Helpers;
using System.Text.RegularExpressions;
using ContactCenter.Infrastructure.Clients.MayTapi;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.Extensions.Configuration;

namespace ContactCenter.Controllers.API
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContactsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Notify _notify;
        private MayTapiClient _mayTapiClient;
        private readonly IConfiguration _configuration;

        public ContactsController(ApplicationDbContext context, Notify notify,  MayTapiClient mayTapiClient, IConfiguration configuration)
        {
            _context = context;
            _notify = notify;
            _mayTapiClient = mayTapiClient;
            _configuration = configuration;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetContacts()
        {
            return await _context.Contacts
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Include(c=>c.ContactFieldValues)
                .ThenInclude(f=>f.Field)
                .ThenInclude(d=>d.DataListValues)
                .Select(q => new ContactDto(q))
                .ToListAsync();
        }

        // GET: api/Contacts/5
        // Busca os dados de um contato pelo seu ID
        // Devolve a hieraraquia dos campos
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDto>> GetContact(string id)
        {

            // Find contact
            ContactDto contactDto = await _context.Contacts
                        .Where(p=>p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select ( p=> new ContactDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (contactDto == null)
            {
                return NotFound();
            }

            // Get all fields that are enabled to Contacts - the authorized group,
            // join field description,
            // then left join field values that were saved to this contact
            var fieldValues = from a in _context.ContactFields
                              join f in _context.Fields on a.FieldId equals f.Id
                              join b in (from d in _context.ContactFieldValues
                                         where d.ContactId == contactDto.Id
                                         select new ContactFieldValue
                                         {
                                             Id = d.Id,
                                             ContactId = d.ContactId,
                                             Field = d.Field,
                                             FieldId = d.FieldId,
                                             Value = d.Value
                                         })
                              on a.FieldId equals b.FieldId into ab
                              from c in ab.DefaultIfEmpty()
                              where a.Enabled & a.GroupId == AuthorizedGroupId()
                              orderby a.Order
                              select new ContactFieldValue { Id = c.Id, ContactId = c.ContactId, FieldId = f.Id, Value = c.Value, Field = f };
                              

            // Adds all Contact Field Values to ContactDto
            foreach ( ContactFieldValue fieldValue in fieldValues)
            {
                contactDto.AddContactFieldValue(fieldValue.Id, fieldValue.FieldId, fieldValue.Value, new FieldDto(fieldValue.Field));
            }

            // Check for DataList types
            foreach (ContactFieldValueDto fieldValue in contactDto.ContactFieldValues)
            {
                // If this FieldValue is connected to a DatList  Field
                if (fieldValue.Field.FieldType == FieldType.DataList)
                {
                    // Gets DataListValues
                    List<DataListValue> dataListValues = await _context.DataListValues
                                        .Where(p => p.FieldId == fieldValue.Field.Id)
                                        .ToListAsync();

                    // Insert DataList at field datalist descriptor
                    fieldValue.Field.DataListValues = dataListValues;
                }
            }

            // Avatar
            contactDto.Avatar = string.IsNullOrEmpty(contactDto.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), contactDto.PictureFileName);

            // return contact
            return contactDto;
        }

        // PUT: api/Contacts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(string id, Contact contact)
        {
            // Check if contact has Id
            if (id != contact.Id | string.IsNullOrEmpty(id))
            {
                string error = "Não foi informado um contato válido com seu ID.";
                return BadRequest(error);
            }

            // Se ApplicatinoUserId for uma string vazia, tem que setado par nulo em função da FK
            if (contact.ApplicationUserId != null && contact.ApplicationUserId.Trim() == string.Empty)
                contact.ApplicationUserId = null;

            // Check if contact Id existe at database
            Contact oldContact = await _context.Contacts
                                        .Where(p => p.Id == contact.Id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

            if ( oldContact == null)
            {
                string error = "Contact {contact.Id} não localizado na base.";
                return NotFound(error);
            }

            // Padroniza ChatChannelId se veio em branco
            if (contact.ChatChannelId == string.Empty)
                contact.ChatChannelId = null;

            // Se foi informado ChatChannel
            if (contact.ChatChannelId != null)
			{
                // Bind ChannelType
                ChatChannel chatChannel = await _context.ChatChannels
                    .Where(p => p.Id == contact.ChatChannelId & p.GroupId == AuthorizedGroupId())
                    .FirstOrDefaultAsync();
                if (chatChannel == null)
                {
                    return NotFound($"ChatChannel {contact.ChatChannelId} não localizado.");
                }
                contact.ChannelType = chatChannel.ChannelType;
            }
            else
			{
                contact.ChannelType = ChannelType.None;
			}

            // Padroniza o celular
            if (!string.IsNullOrEmpty(contact.MobilePhone))
                contact.MobilePhone = Utility.PadronizaCelular(contact.MobilePhone);

            // Bind Group
            contact.GroupId = AuthorizedGroupId();

            // Now check to avoid inserting duplicated fields - When firt PUT is receaved, if ID==0, it will create a new record. 
            foreach (ContactFieldValue contactFieldValue in contact.ContactFieldValues.Where(p => p.Id == 0))
            {
                var existingContactFieldValues = _context.ContactFieldValues.Where(p => p.ContactId == contact.Id & p.FieldId == contactFieldValue.FieldId);
                if (existingContactFieldValues.Any())
                {
                    string error = "já existe um objeto contactFieldValue salvo para o campo {contactFieldValue.FieldId}. Como foi passado o ID=0 no ContactField, essa operação iria criar outro registro, o que não é correto.";
                    return BadRequest(error);
                }

                // Clean FIELD descriptor - otherwise would cascade and alter Field
                contactFieldValue.Field = null;
            }

            // Clean FIELD descriptor - otherwise would cascade and alter Field
            foreach (ContactFieldValue contactFieldValue in contact.ContactFieldValues)
            {
                contactFieldValue.Field = null;
            }

            // If Contact Status is WantingFor Agent, changes to Talking to Agent
            if (contact.Status == ContactStatus.WatingForAgent)
                contact.Status = ContactStatus.TalkingToAgent;

            // Clear unAnsweredCont
            contact.UnAnsweredCount = 0;

            // Check if ApplicationUserID or DepartmentId has changed, and then save to local variables
            string newApplicationUserId = string.Empty;
            string newDepartmentName = string.Empty;
            int newDepartmentId=0;
            if (oldContact.ApplicationUserId != contact.ApplicationUserId) 
            {
                newApplicationUserId = contact.ApplicationUserId;
                contact.Status = ContactStatus.WatingForAgent;
            }
            else if (oldContact.DepartmentId != contact.DepartmentId)
			{
                newDepartmentId = contact.DepartmentId ?? 0;
                contact.Status = ContactStatus.WatingForAgent;
                var newDepartment = await _context.Departments
                                      .Where(p => p.GroupId == AuthorizedGroupId() && p.Id == newDepartmentId)
                                      .FirstOrDefaultAsync();

                newDepartmentName = newDepartment?.Name;
            }

            // Update Database
            _context.Update(contact);

            try
            {
                // Save Changes
                await _context.SaveChangesAsync();

                // If contact was transfered to another Agent
                if (!String.IsNullOrEmpty(newApplicationUserId))
                    // Notify new agent
                    await _notify.NotifyAgent(AuthenticatedUserId(), newApplicationUserId, $"O cliente {contact.Name} foi transferido para seu atendimento.");

                // If contact was trasnfered to another Department
                if (newDepartmentId > 0) 
                {
                    // Notify all agents of corresponding department
                    await _notify.NotifyDepartment(AuthenticatedUserId(), newDepartmentId, $"O cliente {contact.Name} foi transferido para o seu setor.");

                    // Notify contact he/she was transfered to another department
                    await _notify.NotifyContact(contact.Id, $"Você foi transferido para o setor {newDepartmentName}", string.Empty, string.Empty);
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Contacts
        [HttpPost]
        public async Task<ActionResult<ContactDto>> PostContact(Contact contact)
        {

            // Trima o nome
            contact.Name = contact.Name.Trim();

            // Se ApplicatinoUserId for uma string vazia, tem que setado par nulo em função da FK
            if (contact.ApplicationUserId.Trim() == string.Empty)
                contact.ApplicationUserId = null;

            // If ChatChannel is empty, make it null
            if (string.IsNullOrEmpty(contact.ChatChannelId))
			{
                contact.ChatChannelId = null;
                contact.ChannelType = ChannelType.other;
            }

            // Flag que indica se tem whats app
            bool hasWhatsApp = true;

            // Generates a primary Key
            if ( !string.IsNullOrEmpty(contact.MobilePhone) )
            {
                // Padroniza o telefone
                contact.MobilePhone = Utility.PadronizaCelular(contact.MobilePhone);

                // Se tem telefone, Gera Id baseado no telefone
                string phone = contact.MobilePhone;
                if (phone.Length <= 9)
                    return BadRequest("Por favor, digite o telefone com o DDD.");

                else if (phone.Length <= 11)
				{
                    // Confere se tem algum contato com este Id
                    Contact contact1 = _context.Contacts
                                    .Where(p => p.MobilePhone == phone && p.GroupId == AuthorizedGroupId())
                                    .Include(p=>p.ChatChannel)
                                    .Include(p=>p.ApplicationUser)
                                    .AsNoTracking()
                                    .FirstOrDefault();

                    if (contact1 != null)
                    {
                        string msg = $"Já existe na base um contato com este telefone! Nome: {contact1.Name}";
                        if (contact1.ApplicationUser != null)
                            msg += ", atendido por: " + Utility.UppercaseWords(contact1.ApplicationUser.UserName);
                        return Conflict(msg);
                    }
                    else
                        phone = "55" + phone;
                }

                // Guarda os dados do canal que será usado para pesquisar o número
                string checkChannelId = string.Empty;
                string productId = string.Empty;
                string token = string.Empty;

                // Confere o canal atribuido ao cliente
                if ( contact.ChatChannelId != null)
				{
                    ChatChannel chatChannel = await _context.ChatChannels
                        .Where(p => p.Id == contact.ChatChannelId && p.GroupId == AuthorizedGroupId())
                        .FirstOrDefaultAsync();
                    if ( chatChannel !=null)
					{
                        // Amarra o tipo de canal ao contato
                        contact.ChannelType = chatChannel.ChannelType;

                        // Se o canal atribuido é API MayTapi
                        if ( chatChannel.ChannelSubType == ChannelSubType.Alternate2)
						{
                            // Guarda os dados deste canal, para usar o mesmo na validação do número
                            checkChannelId = chatChannel.Id;
                            productId = chatChannel.Login;
                            token = chatChannel.Password;
						}
					}
                    else
					{
                        contact.ChatChannel = null;
					}
				}
                
                // Se o canal atribuído ao contato não é MayTapi, então não guardou o ID para usar na validação do número
                if (string.IsNullOrEmpty(checkChannelId))
				{
                    // Verifica se tem algum canal MayTapi no grupo
                    ChatChannel chatChannel = await _context.ChatChannels
                                            .Where(p => p.GroupId == AuthorizedGroupId() && p.ChannelSubType == ChannelSubType.Alternate2)
                                            .FirstOrDefaultAsync();
                    if ( chatChannel != null)
					{
                        checkChannelId = chatChannel.Id;
                        productId = chatChannel.Login;
                        token = chatChannel.Password;
                    }
                }

                // Se tem um canal MayTapi para validar o numero
                if (!string.IsNullOrEmpty(checkChannelId))
				{
                    // Valida o número
                    CheckNumberRoot checkNumberRoot = await _mayTapiClient.CheckNumber(checkChannelId, phone, productId, token);
                    if (checkNumberRoot.success && checkNumberRoot.result.canReceiveMessage)
                    {
                        phone = checkNumberRoot.result.id.user;
                        contact.Id = AuthorizedGroupId().ToString() + "-" + phone;

                        // Confere se tem algum contato com este Id
                        Contact contact1 = _context.Contacts
                                        .Where(p => p.Id == contact.Id)
                                        .AsNoTracking()
                                        .FirstOrDefault();

                        if (contact1 != null)
						{
                            string msg = $"Já existe na base um contato com este telefone! Nome: {contact1.Name}";
                            if (contact1.ApplicationUser != null)
                                msg += ", atendido por: " + Utility.UppercaseWords(contact1.ApplicationUser.UserName);
                            return Conflict(msg);
                        }

                    }
                    else
                    {
                        // Se nao foi erro de comunicação
                        if ( checkNumberRoot.message == null)
                            // Devolve flag que o numer não tem whats app
                            hasWhatsApp = false;

                        // Gera um ID para o contato
                        contact.Id = AuthorizedGroupId().ToString() + "-" + System.Guid.NewGuid().ToString();
                    }
                }
                else
				{
                    contact.Id = AuthorizedGroupId().ToString() + "-" + System.Guid.NewGuid().ToString();
                }

            }
            else
                // Se não tem telefone, gera Id novo
                contact.Id = AuthorizedGroupId().ToString() + "-" + System.Guid.NewGuid().ToString();

            // Bind Group
            contact.GroupId = AuthorizedGroupId();

            // Bind ApplicationUserId
            contact.ApplicationUserId = AuthenticatedUserId();

            // Bind Department
            ApplicationUser applicationUser = await _context.ApplicationUsers.FindAsync(contact.ApplicationUserId);
            contact.DepartmentId = applicationUser?.DepartmentId;

            // Clear ContactFieldValues
            contact.ContactFieldValues = null;

            // Set first and last activity to now so Contact appears up at left sidebar
            contact.FirstActivity = Utility.HoraLocal();
            contact.LastActivity = Utility.HoraLocal();

            // Set status = Talking to Agent - this is important because side bar list orders by status first
            contact.Status = ContactStatus.TalkingToAgent;

            // Add 
            _context.Contacts.Add(contact);

            // Para retornar
            ContactDto contactDto = new ContactDto(contact);
            contactDto.HasWhatsApp = hasWhatsApp;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ContactExists(contact.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("PostContact", new { id = contact.Id }, contactDto);
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ContactDto>> DeleteContact(string id)
        {
            var contact = await _context.Contacts
                                .Where(p => p.Id == id )
                                .Include(q => q.ContactFieldValues)
                                .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }
            else if ( contact.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return new ContactDto(contact);
        }

        private bool ContactExists(string id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
