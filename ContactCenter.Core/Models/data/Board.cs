using ContactCenter.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    // Board / Sales PipeLine / Ticket PipeLine
    public class Board
    {
        public int Id { get; set; }                                             // Primary Key
        public int? GroupId { get; set; }                                       // Foreing Key to the Group that this Board belongs
        [StringLength(128)]
        public string Name { get; set; }                                        // Board Name
        public string Label { get; set; }                                       // Label, shorten name
        public string CardName { get; set; }                                    // Name of Cards within this Board. Used to create Add buttons.
        public bool AllowMultipleCardsForSameContact { get; set; }              // A contact can be inserted only once at a Sales Pipeline board, but it can be inserted several times at a Support System board
        public int? DepartmentId { get; set; }                                  // Foreing Key to Departments, in case its owned by one. NULL if available to all Agents within Group
        public string ApplicationUserId { get; set; }                           // Foreing Key to Application User, in case this board is owned by a single ApplicationUser
        public virtual ApplicationUser ApplicationUser { get; set; }            // Aplication User descritpr
        public virtual ICollection<Stage> Stages { get; set; }                  // Stages of board
        public virtual ICollection<BoardField> BoardFields { get; set; }        // Fields enabled for this board
        public virtual ICollection<Sending> Sendings { get; set; }              // Envios que usam este quadro
        public virtual ICollection<Filter> Filters { get; set; }                // Filtros salvos para este quadro
        public virtual ICollection<Import> Imports { get; set; }                // Importações

        public void CopyFrom(Board board)
        {
            foreach (PropertyInfo property in typeof(Board).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(this, property.GetValue(board, null), null);
            }
        }
    }

}
