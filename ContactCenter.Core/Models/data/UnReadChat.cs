using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class UnReadChat
    {
        public int Id { get; set; }
        public string FromApplicationUserId { get; set; }
        public string ToApplicationUserId { get; set; }
        public int count { get; set; }
        public DateTime LastDateTime { get; set; }
    }
}
