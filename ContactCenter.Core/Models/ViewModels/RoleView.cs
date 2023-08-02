using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Models.view
{
    public class RoleView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedAt { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserCount { get; set; }
        public string Action { get; set; }
    }
}
