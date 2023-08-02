using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    // Estages of Board
    public class Stage
    {
        public int Id { get; set; }                                        // Primary Key
        public int BoardId { get; set; }                                   // Foreing Key to Board
        public Board Board { get; set; }                                   // Board descriptor
        public string Name { get; set; }                                   // Name
        public string Label { get; set; }                                  // Label, shorten name
        public int Order { get; set; }
        public int ShowMax { get; set; }                                   // When getting cards from stage, Take Top ( showmax )
        public ICollection<Card> Cards { get; set; }                       // Collection of Cards within this Stage
    }
}
