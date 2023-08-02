using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ContactCenter.Core.Models
{

    // Customized Fields diefinition
    // It will be used to configure available fields for Contacts and for Cards
    public class FieldDto
    {
        public FieldDto()
        {

        }
        // Initializes this Dto copying values from another field object
        public FieldDto(Field field)
        {
            // If got parameter field
            if ( field != null)
            {
                // Copy all properties we find in FieldDto from field 
                foreach (PropertyInfo property in typeof(FieldDto).GetProperties().Where(p => p.CanWrite))
                {
                    if (field.GetType().GetProperty(property.Name) != null)
                    {
                        var x = field.GetType().GetProperty(property.Name).GetValue(field, null);
                        property.SetValue(this, x, null);
                    }
                }

            }
        }
        public int Id { get; set; }                                             // Primary Key
        public string Label { get; set; }                                       // Label of the field, for screen use
        public FieldType FieldType { get; set; }                                // Indicate type of field
        public bool IsGlobal { get; set; }                                      // Indicate if this field is available for all Groups, or is particular to a group
        public virtual ICollection<DataListValue> DataListValues { get; set; }  // List of values

    }

}
