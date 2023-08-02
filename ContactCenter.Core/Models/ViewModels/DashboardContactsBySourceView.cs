using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class DashboardContactsBySourceView
    {
        [StringLength(256)]
        public string Id { get; set; }
        [StringLength(256)]
        public string ChannelName { get; set; }
        [StringLength(256)]
        public string ChannelNumber { get; set; }
        [StringLength(256)]
        public string SourceDescription { get; set; }
        public int Qtd { get; set; }
    }
}
