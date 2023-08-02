using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class Filter
    {
        public int Id { get; set; }
        public int GroupId { get; set; }                                                    // Group Id , foreign Key
        public string ApplicationUserId { get; set; }                                       // Foreing Key to Application User
        [StringLength(256)]
        public string Title { get; set; }
        public int? BoardId { get; set; }
        public String JsonFilter { get; set; }
        public virtual ICollection<Sending> Sendings { get; set; }                          // Envios que usam este filtro
    }
}
