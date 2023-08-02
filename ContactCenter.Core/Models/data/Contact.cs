using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    // Status says to who the contact is talking to: bot / wating transfer / agent
    public enum ContactStatus
    {
        TalkingToBot,
        TalkingToAgent,
        WatingForAgent,
    }
    public enum OptStatus
    {
        None,
        OptIn,
        OptOut
    }
    public enum ContactSource
    {
        Unknown,
        Agent,
        ChatChannel,
        Imports,
        Landing
    }
    public class Contact
    {
        public string Id { get; set; }                                              // Primary Key, NOT identiy
        public int GroupId { get; set; }                                            // Group Id , foreign Key
        public virtual Group Group { get; set; }                                    // Group Descriptor
        public virtual ApplicationUser ApplicationUser { get; set; }                // Atendente
        public string Name { get; set; }                                            
        public string FullName { get; set; }                                        
        public string NickName { get; set; }                                        
        public string MobilePhone { get; set; }                                     
        public string Email { get; set; }
        public DateTime FirstActivity { get; set; }                                 // Date and time of first activity - creation
        public DateTime LastActivity { get; set; }                                  // Date and time of last activity
        public string LastText { get; set; }
        public ICollection<ChattingLog> ChattingLogs { get; }                       // list of all Messages sent or receaved by this user
        public ICollection<ExternalAccount> ExternalAccounts { get; }               // list of all externa accounts this user can access on company database
        public ContactStatus Status { get; set; }                                   // indicate who is talking with customer: Bot or Agent
        public int UnAnsweredCount { get; set; }                                    // ananswered messages - by Agent - counter
        public string ApplicationUserId { get; set; }                               // When an Agent talks to customer, Agent ID is saved here
        public int? DepartmentId { get; set; }                                      // If Contact chat is transfered to a given department, its saved here. Can be used to filter Contacts by Department
        public ChannelType ChannelType { get; set; }                                // it will whatsApp Number, bot save 
        public string Channel { get; set; }                                         // additional tag that can be used to identify user in the channel
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Tag3 { get; set; }
        public string ChatChannelId { get; set; }                                           // Foreing Key to Channel.Id - last ChatChannel used by this client
        public virtual ChatChannel ChatChannel { get; set; }                                // ChatChannel Descriptor
        public string PictureFileName { get; set; }                                         // File name of Picture Avatar - saved at clouded Blob Storage
        public virtual ICollection<ContactFieldValue> ContactFieldValues { get; set; }      // Field values for this contact
        public virtual ICollection<Card> Cards { get; set; }
        public OptStatus OptStatus { get; set; }                                            // Opted In / Out for notifications
        public ContactSource Source { get; set; }                                           // Source of contact - where it came from

        // Used by API to clone contact
        public void CopyFrom(Contact contact)
        {
            foreach (PropertyInfo property in typeof(Contact).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(this, property.GetValue(contact, null), null);
            }
        }
    }
}
