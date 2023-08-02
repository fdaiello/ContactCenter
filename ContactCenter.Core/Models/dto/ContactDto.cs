using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    public class ContactDto
    {
        public ContactDto(Contact contact)
        {
            if ( contact != null )
            {
                foreach (PropertyInfo property in typeof(ContactDto).GetProperties().Where(p => p.CanWrite & p.Name != "HasWhatsApp" & p.Name != "Avatar"))
                {
                    var x = contact.GetType().GetProperty(property.Name).GetValue(contact, null);
                    property.SetValue(this, x, null);
                }

                this.ContactFieldValues = new Collection<ContactFieldValueDto>();

                if (contact.ContactFieldValues != null)
                {
                    foreach (ContactFieldValue contactFieldValue in contact.ContactFieldValues)
                    {
                        this.AddContactFieldValue(contactFieldValue.Id, contactFieldValue.FieldId, contactFieldValue.Value, new FieldDto(contactFieldValue.Field));
                    }
                }
            }
        }

        public string Id { get; set; }                                                  // Primary Key, NOT identiy
        public string Name { get; set; }                                            
        public string FullName { get; set; }                                        
        public string NickName { get; set; }                                        
        public string MobilePhone { get; set; }                                     
        public string Email { get; set; }
        public DateTime FirstActivity { get; set; }                                     // Date and time of first activity - creation
        public DateTime LastActivity { get; set; }                                      // Date and time of last activity
        public string LastText { get; set; }
        public ContactStatus Status { get; set; }                                       // indicate who is talking with customer: Bot or Agent
        public int UnAnsweredCount { get; set; }                                        // ananswered messages - by Agent - counter
        public string ApplicationUserId { get; set; }                                   // When an Agent talks to customer, Agent ID is saved here
        public int? DepartmentId { get; set; }                                          // If Contact chat is transfered to a given department, its saved here. Can be used to filter Contacts by Department
        public string PictureFileName { get; set; }                                     // File name of Picture Avatar - saved at clouded Blob Storage
        public string Avatar { get; set; }                                              // PictureFileName with server path
        public ChannelType ChannelType { get; set; }                                    // it will whatsApp Number, bot save 
        public string ChatChannelId { get; set; }                                       // Foreing Key to Channel.Id - last ChatChannel used by this client
        public ICollection<ContactFieldValueDto> ContactFieldValues { get;  }           // Field values for this contact
        public virtual ApplicationUser ApplicationUser { get; set; }                    // Atendente
        public ContactSource Source { get; set; }                                       // Source of contact - where it came from

        public void AddContactFieldValue(int id, int fieldId, string value, FieldDto fieldDto)
        {
            this.ContactFieldValues.Add(new ContactFieldValueDto { Id = id, FieldId = fieldId, Value = value, Field = fieldDto });
        }
        public bool HasWhatsApp { get; set; }

    }
}
