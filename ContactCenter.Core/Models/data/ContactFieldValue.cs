using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Contact Field Value
    // Holds the effective value of a field for a given Contact
    public class ContactFieldValue
    {
        public int Id { get; set; }                            // Primary Key
        public int FieldId { get; set; }                       // Field Foreign Key
        public virtual Field Field { get; set; }               // Field descriptor
        public string ContactId { get; set; }                  // Foreign Key to Contact
        public string Value { get; set; }                      // Value for the field
    }
}
