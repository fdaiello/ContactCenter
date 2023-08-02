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
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CardsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CardsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDto>>> GetCards(string contactId, int boardId)
        {
            List<CardDto> cardDtos;

            if (boardId > 0)
            {
                // Gets All cards from given Board
                cardDtos = await _context.Cards
                    .Where(p => p.Stage.Board.GroupId == AuthorizedGroupId() & p.Stage.BoardId == boardId)
                    .Include( p=> p.CardFieldValues)
                    .ThenInclude( p=>p.Field)
                    .Include(s => s.Stage)
                    .Include(c => c.Contact)
                    .ThenInclude( x=> x.ApplicationUser)
                    .AsNoTracking()
                    .OrderBy(o => o.Order)
                    .Select(q => new CardDto(q))
                    .ToListAsync();
            }
            else if (string.IsNullOrEmpty(contactId))
            {
                // Gets All cards
                cardDtos = await _context.Cards
                    .Where(p => p.Stage.Board.GroupId == AuthorizedGroupId())
                    .Include(p => p.CardFieldValues)
                    .ThenInclude(p => p.Field)
                    .Include(c => c.Contact)
                    .Include(s => s.Stage)
                    .AsNoTracking()
                    .Select(q => new CardDto(q))
                    .ToListAsync();
            }
            else
            {
                // Gets Cards for a given Contact
                cardDtos = await _context.Cards
                    .Where(p => p.ContactId == contactId & p.Contact.GroupId == AuthorizedGroupId())
                    .Include(p => p.CardFieldValues)
                    .ThenInclude(p => p.Field)
                    .Include(c => c.Contact)
                    .Include(s => s.Stage)
                    .ThenInclude( b=> b.Board)
                    .AsNoTracking()
                    .Select(q => new CardDto(q))
                    .ToListAsync();
            }

            // Para todos os cartões
            foreach (CardDto cardDto in cardDtos)
            {
                // Vamos completar caminho no avatar, ou se nao houver, inserimos o nome do contato, indicando que deve ser gerado o icone pelo nome
                cardDto.Contact.PictureFileName = string.IsNullOrEmpty(cardDto.Contact.PictureFileName) ? ( cardDto.Contact.FullName ?? cardDto.Contact.Name) : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), cardDto.Contact.PictureFileName);

                // Idem para o avatar do atendente - se estiver na coleção
                if ( cardDto.Contact.ApplicationUser != null )
                    cardDto.Contact.ApplicationUser.PictureFile = string.IsNullOrEmpty(cardDto.Contact.ApplicationUser.PictureFile) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), cardDto.Contact.ApplicationUser.PictureFile);

            }
 
            return cardDtos;
        }

        // GET: api/Cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CardDto>> GetCard(int id)
        {
            // Check if card Exists
            var card = await _context.Cards
                            .Where(p=>p.Id == id)
                            .Include(co => co.Contact)
                            .Include(s=>s.Stage)
                            .ThenInclude(p=>p.Board)
                            .FirstOrDefaultAsync();

            if (card == null)
            {
                return NotFound();
            }

            // Check if Id belongs to Authorized Group
            if ( card.Stage.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            // Vamos adicionar o caminho no avatar
            card.Contact.PictureFileName = string.IsNullOrEmpty(card.Contact.PictureFileName) ? string.Empty : Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), card.Contact.PictureFileName);

            // Create a CardDto from this card
            var cardDto = new CardDto(card);

            // Get all fields that are enabled to the Board this card belongs
            // join field description,
            // then left join field values that were saved to this card
            var cardFieldValues = from a in _context.BoardFields
                              join f in _context.Fields on a.FieldId equals f.Id
                              join b in (from d in _context.CardFieldValues
                                         where d.CardId == card.Id
                                         select new CardFieldValue
                                         {
                                             Id = d.Id,
                                             CardId = d.CardId,
                                             Field = d.Field,
                                             FieldId = d.FieldId,
                                             Value = d.Value
                                         })
                              on a.FieldId equals b.FieldId into ab
                              from c in ab.DefaultIfEmpty()
                              where a.BoardId == card.Stage.BoardId & a.Enabled 
                              orderby a.Order
                              select new CardFieldValue { Id = c.Id, CardId = c.CardId, FieldId = f.Id, Value = c.Value, Field = f };

            // Add card field values to CardDto
            foreach (CardFieldValue cardFieldValue in cardFieldValues)
            {
                cardDto.AddCardFieldValue(cardFieldValue);
            }

            // Check for DataList types
            foreach (CardFieldValueDto cardFieldValue in cardDto.CardFieldValues)
            {
                // If this FieldValue is connected to a DatList  Field
                if (cardFieldValue.Field.FieldType == FieldType.DataList)
                {
                    // Gets DataListValues
                    List<DataListValue> dataListValues = await _context.DataListValues
                                        .Where(p => p.FieldId == cardFieldValue.Field.Id)
                                        .ToListAsync();

                    // Insert DataList at field datalist descriptor
                    cardFieldValue.Field.DataListValues = dataListValues;
                }
            }


            // Return
            return cardDto;
        }

        // GET: api/Cards/newtemplate
        [HttpGet("NewTemplate")]
        public async Task<ActionResult<NewCardDto>> GetNewCardTemplate(int boardId)
        {
            // Check if card Exists
            var card = new Card();


            // Create a CardDto from this card
            var cardDto = new NewCardDto();

            // Get all fields that are enabled to the Board this card belongs
            // join field description,
            var cardFieldValues = from a in _context.BoardFields
                                  join f in _context.Fields on a.FieldId equals f.Id
                                  where a.Enabled & a.Board.GroupId == AuthorizedGroupId() & a.BoardId == boardId
                                  orderby a.Order
                                  select new CardFieldValue { Id =0, CardId = 0, FieldId = f.Id, Value = string.Empty, Field = f };

            // Add card field values to CardDto
            foreach (CardFieldValue cardFieldValue in cardFieldValues)
            {
                cardDto.AddCardFieldValue(cardFieldValue);
            }

            // Check for DataList types
            foreach (CardFieldValueDto cardFieldValue in cardDto.CardFieldValues)
            {
                // If this FieldValue is connected to a DatList  Field
                if (cardFieldValue.Field.FieldType == FieldType.DataList)
                {
                    // Gets DataListValues
                    List<DataListValue> dataListValues = await _context.DataListValues
                                        .Where(p => p.FieldId == cardFieldValue.Field.Id)
                                        .ToListAsync();

                    // Insert DataList at field datalist descriptor
                    cardFieldValue.Field.DataListValues = dataListValues;
                }
            }

            // Check first stage available to this board
            Stage stage = _context.Stages
                            .Where(p => p.BoardId == boardId)
                            .OrderBy(o => o.Order).ThenBy(o=>o.Id)
                            .FirstOrDefault();
            if ( stage != null)
                cardDto.StageId = stage.Id;

            // Return
            return cardDto;
        }
        // PUT: api/Cards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCard(int id, Card card)
        {
            if (id != card.Id)
            {
                return BadRequest();
            }

            // Check if Card Stage exists and belongs to a Board that belongs to current Group
            var stage = await _context.Stages
                                .Where(p => p.Id == card.StageId)
                                .AsNoTracking()
                                .Include(b => b.Board)
                                .FirstOrDefaultAsync();

            if ( stage == null)
            {
                return NotFound();
            }
            else if ( stage.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            // Clean CONTACT descriptor field
            card.Contact = null;

            // Clean STAGE descriptor
            card.Stage = null;

            // Clean FIELD descriptor
            if ( card.CardFieldValues != null)
            {
                foreach ( CardFieldValue cardFieldValue in card.CardFieldValues)
                {
                    cardFieldValue.Field = null;
                }
            }

            // Now check to avoid inserting duplicated fields - When firt PUT is receaved, if ID==0, it will create a new record. 
            foreach ( CardFieldValue cardFieldValue1 in card.CardFieldValues.Where(p=>p.Id == 0))
            {
                var existingCardFieldValues = _context.CardFieldValues.Where(p => p.CardId == card.Id & p.FieldId == cardFieldValue1.FieldId);
                if (existingCardFieldValues.Any())
                {
                    throw (new SystemException($"Error: cardFieldValue for Field {cardFieldValue1.FieldId} already exist at database for this card. As Id is 0 this operation would try to create another record for same field."));
                }
            }

            // UpdatedDate
            card.UpdatedDate = Utility.HoraLocal();

            // Update
            _context.Update(card);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CardExists(id))
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

        // POST: api/Cards
        [HttpPost]
        public async Task<ActionResult<CardDto>> PostCard(Card card)
        {

            // When inserting Card, Id must be 0, as this is an autogenerated field;
            if (card.Id != 0)
            {
                return BadRequest();
            }

            // Check if Card Stage exists and belongs to a Board that belongs to current Group
            var stage = await _context.Stages
                                .Where(p => p.Id == card.StageId)
                                .Include(b => b.Board)
                                .FirstOrDefaultAsync();

            if (stage == null || stage.Id == 0)
            {
                return BadRequest("Não foi cadastrado nenhum estágio neste quadro!");
            }
            else if (stage.Board.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            // CreatedDate
            card.CreatedDate = Utility.HoraLocal();
            card.UpdatedDate = Utility.HoraLocal();

            // Clean CONTACT descriptor field
            var tmpContact = card.Contact;
            card.Contact = null;

            // Color default
            if ( card.Color == null)
            {
                card.Color = "white";
            }

            if (card.CardFieldValues != null)
            {
                foreach (CardFieldValue cardFieldValue in card.CardFieldValues)
                {
                    // Clean FIELD descriptor
                    cardFieldValue.Field = null;
                }
            }

            // Add Card
            _context.Cards.Add(card);

            // Save Card
            await _context.SaveChangesAsync();

            // Put CONTACT descriptor back just to return to user
            card.Contact = tmpContact;

            // Return card
            return CreatedAtAction("PostCard", new { id = card.Id }, new CardDto(card));
        }

        // DELETE: api/Cards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Card>> DeleteCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                return NotFound();
            }

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return card;
        }

        private bool CardExists(int id)
        {
            return _context.Cards.Any(e => e.Id == id);
        }
    }
}
