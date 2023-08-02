using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContactCenter.Core.Models;

namespace ContactCenter.Core.Models
{
    public class DepartmentDto
    {
        public DepartmentDto(Department department)
        {
            foreach (PropertyInfo property in typeof(DepartmentDto).GetProperties().Where(p => p.CanWrite))
            {
                var x = department.GetType().GetProperty(property.Name).GetValue(department, null);
                property.SetValue(this, x, null);
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserCount { get; set; }
    }
}
