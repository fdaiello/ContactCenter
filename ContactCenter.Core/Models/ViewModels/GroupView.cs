using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Models.view
{
    public class GroupView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedAt { get; set; }
        public string Descr { get; set; }
        public int UserCount { get; set; }
        public string Action { get; set; }
        public Uri BotUrl { get; set; }
    }
}
