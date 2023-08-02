using System;
using System.Collections.Generic;

namespace ContactCenter.Core.Models
{
    /**
     * This is User's Group Model.
     * Id:   index
     * Name: group name
     */
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Descr { get; set; }
        public int UserCount { get; set; }
        public Uri BotUrl { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<ChatChannel> ChatChannels { get; set; }
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual ICollection<WeekSchedule> WeekSchedules { get; set; }
        public virtual ICollection<Board> Board { get; set; }
        public virtual ICollection<Field> Fields { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Sending> Sendings { get; set; }
        public virtual ICollection<Import> Imports { get; set; }
    }
}
