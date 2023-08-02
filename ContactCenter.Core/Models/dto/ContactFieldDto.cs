using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Contact Field - Indicate a field that is available to use for Contact Detail
    // allways for a given group
    public class ContactFieldDto
    {
        public ContactFieldDto(ContactField contactField)
        {
            if ( contactField != null)
            {
                foreach (PropertyInfo property in typeof(ContactFieldDto).GetProperties().Where(p => p.CanWrite & p.Name != "Field"))
                {
                    var x = contactField.GetType().GetProperty(property.Name).GetValue(contactField, null);
                    property.SetValue(this, x, null);
                }
                if (contactField.Field != null)
                {
                    this.Field = new FieldDto(contactField.Field);
                }
            }
        }
        public int Id { get; set; }                             // Primary Key
        public int FieldId { get; set; }                        // Foreing Key to Field
        public bool Enabled { get; set; }                       // Indicate if this field is enabled 
        public int Order { get; set; }                          // Sort Order
        public virtual FieldDto Field { get; set; }             // Field descriptor

    }
}
