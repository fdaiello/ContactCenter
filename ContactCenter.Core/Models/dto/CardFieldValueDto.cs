using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{ 
    public class CardFieldValueDto
    {
        public CardFieldValueDto(CardFieldValue cardFieldValue)
        {
            Id = cardFieldValue.Id;
            FieldId = cardFieldValue.FieldId;
            Field = new FieldDto(cardFieldValue.Field);
            CardId = cardFieldValue.CardId;
            Value = cardFieldValue.Value;
        }
        public int Id { get; set; }                           
        public int FieldId { get; set; }
        public virtual FieldDto Field { get; set; } = new FieldDto();
        public int CardId { get; set; }                       
        public string Value { get; set; }                     

    }
}
