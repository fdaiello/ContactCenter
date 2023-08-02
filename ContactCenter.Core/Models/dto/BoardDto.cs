using ContactCenter.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    // Board / Sales PipeLine / Ticket PipeLine
    public class BoardDto
    {
        public BoardDto( Board board)
        {
            // If BoardDto constructor got a valid board to initialize
            if ( board != null)
            {
                // Copy all values from board passed as parameter
                foreach (PropertyInfo property in typeof(BoardDto).GetProperties().Where(p => p.CanWrite))
                {
                    var x = board.GetType().GetProperty(property.Name).GetValue(board, null);
                    property.SetValue(this, x, null);
                }

                // Initialize Stages collection
                this.Stages = new Collection<StageDto>();

                // If original board has Stages
                if (board.Stages != null)
                {
                    // Copy all stages from original Board
                    foreach (Stage stage in board.Stages)
                    {
                        this.AddStage(stage);
                    }
                }

                // Initialize BoardFields collection
                this.BoardFields = new Collection<BoardFieldDto>();

                // Copy all stages
                if (board.BoardFields != null)
                {
                    // Copies all board fields from original Board 
                    foreach (BoardField boardField in board.BoardFields)
                    {
                        this.AddBoardField(boardField);
                    }
                }

            }
        }
        public int Id { get; set; }                                             // Primary Key
        [StringLength(128)]
        public string Name { get; set; }                                        // Board Name
        public string Label { get; set; }                                       // Label, shorten name
        public string CardName { get; set; }                                    // Name of Cards within this Board. Used to create Add buttons.
        public bool AllowMultipleCardsForSameContact { get; set; }              // A contact can be inserted only once at a Sales Pipeline board, but it can be inserted several times at a Support System board
        public int? DepartmentId { get; set; }                                  // Foreing Key to Departments, in case its owned by one. 0 if available to all Agents within Group
        public string ApplicationUserId { get; set; }                           // Foreing Key to Application User, in case this board is owned by a single ApplicationUser
        public virtual ICollection<StageDto> Stages { get;  }                   // Stages of board
        public virtual ICollection<BoardFieldDto> BoardFields { get; }          // Fields enabled for this board
        public void AddStage(Stage stage)
        {
            this.Stages.Add(new StageDto(stage));
        }
        public void AddBoardField(BoardField boardField)
        {
            this.BoardFields.Add(new BoardFieldDto(boardField));
        }
    }

}
