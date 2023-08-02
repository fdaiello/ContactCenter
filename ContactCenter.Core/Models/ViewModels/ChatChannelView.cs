using ContactCenter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Models.view
{
    public class ChatChannelView
    {
        public string Id { get; set; }                            
        public int GroupId { get; set; }                         
        public string Name { get; set; }
        public string Alias { get; set; }
        public string PhoneNumber { get; set; }
        public string AppName { get; set; }
        public ChannelType ChannelType { get; set; }
        public ChannelSubType ChannelSubType { get; set; }
        public int? DepartmentId { get; set; }                      
        public string ApplicationUserId { get; set; }    
        public string Host { get; set; }
        public string Login { get; set; }                        
        public string Password { get; set; }        
        public string Status { get; set; }
        public string TypeDescr { get; set; }
        public string Icon { get; set; }
        public string From { get; set; }
    }
}
