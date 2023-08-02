using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Contact Field - Indicate a field that is available to use for Contact Detail
    // allways for a given group
    public class ContactField
    {
        public int Id { get; set; }                             // Primary Key
        public int GroupId { get; set; }                        // Foreing Key to Group
        public virtual Group Group { get; set; }                // Group Descriptor
        public int FieldId { get; set; }                        // Foreing Key to Field
        public virtual Field Field { get; set; }                // Field descriptor
        public bool Enabled { get; set; }                       // Indicate if this field is enabled 
        public int Order { get; set; }                          // Sort Order


    }
}
