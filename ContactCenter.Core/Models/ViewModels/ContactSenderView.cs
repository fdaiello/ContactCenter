using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    public class ContactSenderView
    {
        public string Id { get; set; }                                              // Primary Key, NOT identiy
        public string Name { get; set; }                                            
        public string MobilePhone { get; set; }                                     
        public string Email { get; set; }
    }
}
