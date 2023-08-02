using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    // Data List Value
    // Single value ( entry ) available for a give DataList
    public class DataListValueDto
    {
        public DataListValueDto(DataListValue dataListValue)
        {
            foreach (PropertyInfo property in typeof(DataListValueDto).GetProperties().Where(p => p.CanWrite))
            {
                var x = dataListValue.GetType().GetProperty(property.Name).GetValue(dataListValue, null);
                property.SetValue(this, x, null);
            }
        }
        public DataListValueDto()
        {

        }
        public int Id { get; set; }
        public int DataListId { get; set; }                                     // DataList Foreign Key
        public string Value { get; set; }
    }
}
