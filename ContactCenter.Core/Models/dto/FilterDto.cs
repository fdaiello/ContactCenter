using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class FilterDto
    {

        // Initializes this Dto copying values from another filter object
        public FilterDto(Filter filter)
        {
            // If got parameter field
            if (filter != null)
            {
                // Copy all properties we find in FilterDto from filter
                foreach (PropertyInfo property in typeof(FilterDto).GetProperties().Where(p => p.CanWrite))
                {
                    if (filter.GetType().GetProperty(property.Name) != null)
                    {
                        var x = filter.GetType().GetProperty(property.Name).GetValue(filter, null);
                        property.SetValue(this, x, null);
                    }
                }

            }
        }
        public int Id { get; set; }
        [StringLength(256)]
        public string Title { get; set; }
        public int? BoardId { get; set; }
        public string ApplicationUserId { get; set; }                                       // Foreing Key to Application User
        public String JsonFilter { get; set; }
        public virtual ICollection<Sending> Sendings { get; set; }                          // Envios que usam este filtro
    }
}
