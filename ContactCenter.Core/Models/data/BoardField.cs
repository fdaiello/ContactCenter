using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Enabled Field - Indicate available custom field for cards of a given Board
    public class BoardField
    {
        public int Id { get; set; }                             // Primary Key
        public int FieldId { get; set; }                        // Foreing Key to Field
        public virtual Field Field { get; set; }                // Field descriptor
        public int BoardId { get; set; }                        // Foreing Key to Board
        public Board Board { get; set; }                        // Board descriptor
        public bool Enabled { get; set; }                       // Indicate if this field is enabled 
        public int Order { get; set; }                          // Sort Order

    }
}
