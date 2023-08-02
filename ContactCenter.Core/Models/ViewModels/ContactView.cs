using ContactCenter.Core.Models;
using System;

namespace ContactCenter.Core.Models
{
    public class ContactView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LastMessage { get; set; }
        public string LastTime { get; set; }
        public string Avatar { get; set; }
        public string Roles { get; set; }
        public ChannelType ChannelType { get; set; }
        public string Channel { get; set; }
        public int UnAnsweredCount { get; set; }
        public ContactStatus Status { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ApplicationUserId { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
