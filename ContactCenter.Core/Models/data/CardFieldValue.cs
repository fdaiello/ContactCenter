using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Card Field Value
    // Holds the effective value of a filed for a given Card ( contact in stage of given board );
    public class CardFieldValue
    {
        public int Id { get; set; }                           // Primary Key
        public int FieldId { get; set; }                      // Field Foreign Key
        public virtual Field Field { get; set; }              // Field descriptor
        public int CardId { get; set; }                       // Foreign key to Card
        public virtual Card Card { get; set; }                // Card descriptor  
        public string Value { get; set; }                     // Value for the field

    }
}
