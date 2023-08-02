using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
    public class MessageDto
    {
        public MessageDto( Message message)
        {
            if (message != null)
            {
                foreach (PropertyInfo property in typeof(MessageDto).GetProperties())
                {
                    var x = message.GetType().GetProperty(property.Name).GetValue(message, null);
                    property.SetValue(this, x, null);
                }
            }
        }
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Uri FileUri { get; set; }
        public MessageType MessageType { get; set; }
        public string Html { get; set; }
        public string SmartCode { get; set; }
        public int? SmartIndex { get; set; }
        public string Thumbnail { get; set; }
    }
}

