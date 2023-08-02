using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class SendingReportView
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int Total { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Read { get; set; }
        public int Failed { get; set; }
    }
}
