using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    public class ChattingLogView    {
        public long Id { get; set; }                            // Primary Key
        public ChattingLog ChattingLog { get; set; }
        public string AgentName { get; set; }
        public string ContactName { get; set; }
        public string ToAgentName { get; set; }
        public ChattingLog QuotedLog { get; set; }
    }
}
