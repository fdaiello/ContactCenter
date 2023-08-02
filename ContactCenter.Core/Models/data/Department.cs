using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ContactCenter.Core.Models;

namespace ContactCenter.Core.Models
{
    public class Department
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        [StringLength(1024)]
        public string Alias { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserCount { get; set; }
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<WeekSchedule> WeekSchedules { get; set; }
        public virtual ICollection<ChatChannel> ChatChannels { get; set; }
    }
}
