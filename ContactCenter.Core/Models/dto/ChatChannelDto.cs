using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    public class ChatChannelDto
    {

        public ChatChannelDto(ChatChannel chatChannel)
        {
            // Copies all fields from original ChatChannel to this new ChatChannelDto object
            foreach (PropertyInfo property in typeof(ChatChannelDto).GetProperties().Where(p => p.CanWrite))
            {
                var x = chatChannel.GetType().GetProperty(property.Name).GetValue(chatChannel, null);
                property.SetValue(this, x, null);
            }
        }
        public string Id { get; set; }                              // Primary Key, NOT identiy
        public string Name { get; set; }                            
        public string PhoneNumber { get; set; }
        public string AppName { get; set; }
        public ChannelType ChannelType { get; set; }                
        public ChannelSubType ChannelSubType { get; set; }      
        public int? DepartmentId { get; set; }                      // Foreing Key Department.Id [Optional]
        public string ApplicationUserId { get; set; }               // Foreing Key ApplicationUser.Id [Optional]
        public string Login { get; set; }                           // For channels that need to authenticate, as instagram
        public string Password { get; set; }
        public string Status { get; set; }                          // Status of the Channel. For Wassenger Whats App, says if its online
        public int? BotSettingsId { get; set; }                     // Id do BotSettings vinculado a este canal
        public String Host { get; set; }                            // For SMTP Channels, saves Host address
        public String From { get; set; }                            // For SMTP Channels, saves From address
        public bool SaveGroupMessages { get; set; }
        public bool EnableTextToSpeech { get; set; }
    }
}
