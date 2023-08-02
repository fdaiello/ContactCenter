using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Enabled Field - Indicate available custom field for cards of a given Board
    public class BoardFieldDto
    {
        public BoardFieldDto(BoardField boardField)
        {
            if ( boardField != null)
			{
                // Copies all fields that exists at the DTO from original boardField
                foreach (PropertyInfo property in typeof(BoardFieldDto).GetProperties().Where(p => p.CanWrite))
                {
                    var x = boardField.GetType().GetProperty(property.Name).GetValue(boardField, null);
                    property.SetValue(this, x, null);
                }

                // Copies Field descriptor
                this.Field = new FieldDto(boardField.Field);
            }
        }
        public int Id { get; set; }                             // Primary Key
        public int FieldId { get; set; }                        // Foreing Key to Field
        public int BoardId { get; set; }                        // Foreing Key to Board
        public bool Enabled { get; set; }                       // Indicate if this field is enabled 
        public int Order { get; set; }                          // Sort Order
        public virtual FieldDto Field { get;  }                 // Field descriptor

    }
}
