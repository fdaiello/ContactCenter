using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{

    public class NewCardDto
    {
        public NewCardDto()
        {
            this.CardFieldValues = new Collection<CardFieldValueDto>();
        }
        public int Id { get; set; }                                                  
        public string ContactId { get; set; }
        public int StageId { get; set; }
        public string Color { get; set; }
        public ICollection<CardFieldValueDto> CardFieldValues { get; }
        public DateTime CreatedDate { get; set; }                             
        public DateTime UpdatedDate { get; set; }                             
        public void AddCardFieldValue(CardFieldValue cardFieldValue)
        {
            this.CardFieldValues.Add(new CardFieldValueDto (cardFieldValue));
        }
    }

}
