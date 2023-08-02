using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{

    public class CardDto
    {
        public CardDto(Card card)
        {
            // Copies all fields from original card to this new CardDto object
            foreach (PropertyInfo property in typeof(CardDto).GetProperties().Where(p => p.Name != nameof(CardFieldValues) && p.Name != nameof(Contact)))
            {
                var x = card.GetType().GetProperty(property.Name).GetValue(card, null);
                property.SetValue(this, x, null);
            }

            // Initializes Collecttion
            this.CardFieldValues = new Collection<CardFieldValueDto>();

            // If original card as CardFieldValues
            if (card.CardFieldValues != null)
            {
                // Copy all cardfieldvalues from original card
                foreach (CardFieldValue cardFieldValue in card.CardFieldValues.OrderBy(p=>p.Field == null ? 0 : p.Field.Id))
                {
                    this.AddCardFieldValue(cardFieldValue);
                }
            }

            // Copies card.Contact descriptor
            this.Contact = new ContactDto(card.Contact);

        }

        public int Id { get; set; }                                                  
        public string ContactId { get; set; }
        public ContactDto Contact { get;  }
        public int StageId { get; set; }
        public virtual Stage Stage { get; set; }                                // Stage descriptor
        public string Color { get; set; }
        public int Order { get; set; }
        public ICollection<CardFieldValueDto> CardFieldValues { get; set; }
        public DateTime CreatedDate { get; set; }                             
        public DateTime UpdatedDate { get; set; }                             
        public void AddCardFieldValue(CardFieldValue cardFieldValue)
        {
            this.CardFieldValues.Add(new CardFieldValueDto (cardFieldValue));
        }
    }

}
