using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    // Card
    // Indicates a contat that is posted at a Board, on a given Stage
    public class Card
    {
        public int Id { get; set; }                                             // Primary Key
        public string ContactId { get; set; }                                   // Foreign Key to Contacts ( former Customer )
        public virtual Contact Contact { get; set; }                            // Contact Descritor
        public int StageId { get; set; }                                        // Foreing Key to Stage
        [StringLength(16)]
        public string Color { get; set; }                                       // Card color
        public virtual Stage Stage { get; set; }                                // Stage descriptor
        public int Order { get; set; }
        public ICollection<CardFieldValue> CardFieldValues { get; set; }        // Field Value for this card
        public DateTime CreatedDate { get; set; }                               // Date time when it was created
        public DateTime UpdatedDate { get; set; }                               // Date time when it was altered last
    }
}
