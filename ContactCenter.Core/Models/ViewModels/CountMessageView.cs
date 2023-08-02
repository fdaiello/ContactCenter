using System;

namespace ContactCenter.Core.Models
{
    public class CountMessageView
    {
        public string MessageDate { get; set; }
        public string ContactId { get; set; }
        public string ApplicationUserId { get; set; }
        public int MessageCount { get; set; }
    }
}
