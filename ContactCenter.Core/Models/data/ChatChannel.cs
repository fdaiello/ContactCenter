using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContactCenter.Core.Models
{
    public enum ChannelType
    {
        None,
        WhatsApp,
        WebChat,
        Messenger,
        Emulator,
        Instagram,
        other,
        Email,
        SMS
    }
    public enum ChannelSubType
    {
        None,
        Oficial,                        // GupShup
        Alternate1,                     // Wassenger
        Alternate2                      // MayTapi
    }


    public class ChatChannel
    {
        [StringLength(32)]
        public string Id { get; set; }                              // Primary Key, NOT identiy
        public int GroupId { get; set; }                            // Group Id , foreign Key
        public string Name { get; set; }                            
        public string PhoneNumber { get; set; }
        public string AppName { get; set; }
        public ChannelType ChannelType { get; set; }                
        public ChannelSubType ChannelSubType { get; set; }      
        public int? DepartmentId { get; set; }                      // Foreing Key Department.Id [Optional]
        public string ApplicationUserId { get; set; }               // Foreing Key ApplicationUser.Id [Optional]
        public string Login { get; set; }                           // For channels that need to authenticate, as instagram and email
        public string Password { get; set; }
        [StringLength(16)]
        public string Status { get; set; }                          // Status of the Channel. For Wassenger Whats App, says if its online
        public int? BotSettingsId { get; set; }                     // Id do BotSettings vinculado a este canal
        public virtual BotSettings BotSettings { get; set; }        // Bot Settings Descriptor
        public bool SaveGroupMessages { get; set; }
        public bool EnableTextToSpeech { get; set; }
        public virtual ICollection<Sending> Sendings { get; set; }
        [StringLength(128)]
        public String Host { get; set; }                            // For SMTP Channels, saves Host address
        [StringLength(128)]
        public String From { get; set; }                            // For SMTP Channels, saves From address
    }
}
