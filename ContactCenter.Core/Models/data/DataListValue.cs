using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Data List Value
    // Single value ( entry ) available for a give DataList
    public class DataListValue
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
    }
}
