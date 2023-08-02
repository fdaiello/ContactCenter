using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class ContactFieldValueDto
    {
        public int Id { get; set; }                                         
        public int FieldId { get; set; }                       
        public FieldDto Field { get; set; } = new FieldDto();  
        public string Value { get; set; }                      
    }
}
