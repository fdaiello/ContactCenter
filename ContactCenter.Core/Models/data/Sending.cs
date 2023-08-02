using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactCenter.Core.Models
{
    public enum SendingStatus
	{
        Queued,
        Sending,
        Sent,
        Canceled
	}
    public class Sending
    {
        public int Id { get; set; }
        public SendingStatus Status { get; set; }
        public int GroupId { get; set; }                                                    // Group Id , foreign Key
        [StringLength(256)]
        public string Title { get; set; }
        public MessageType MessageType { get; set; }
        public int? BoardId { get; set; }
        public int? FilterId { get; set; }
        [StringLength(32)]
        public string ChatChannelId { get; set; }
        public int MessageId { get; set; }
        public int? SmartPageId { get; set; }
        public virtual Board Board { get; set; }
        public virtual Filter Filter { get; set; }
        public virtual ChatChannel ChatChannel { get; set; }
        public virtual Message Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public virtual ICollection<ChattingLog> ChattingLogs { get; set; }
        public int QtdContacts { get; set; }
        public int QtdSent { get; set; }
    }
}
