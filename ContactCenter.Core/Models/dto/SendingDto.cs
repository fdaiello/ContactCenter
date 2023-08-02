using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class SendingDto
    {
        public SendingDto(Sending sending)
        {
            if (sending != null)
            {
                foreach (PropertyInfo property in typeof(SendingDto).GetProperties())
                {
                    var x = sending.GetType().GetProperty(property.Name).GetValue(sending, null);
                    property.SetValue(this, x, null);
                }
            }
        }
        public int Id { get; set; }
        public SendingStatus Status { get; set; }

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
        public virtual Message Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public int QtdContacts { get; set; }
        public int QtdSent { get; set; }
    }
}
