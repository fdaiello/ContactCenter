using System;
using System.Collections.Generic;
using DBConverter.Models.data.Interfaces;
using Microsoft.AspNetCore.Identity;
using ContactCenter.Core.Models;

namespace ContactCenter.Core.Models
{
    public enum NotificationLevel
	{
        None,
        FromAgents,
        FromAgentsAndOwnContacts,
        All
	}
    public class ApplicationUser : IdentityUser, IAuditableEntity
    {
        public virtual string FriendlyName
        {
            get
            {
                string friendlyName = string.IsNullOrWhiteSpace(FullName) ? UserName : FullName;

                if (!string.IsNullOrWhiteSpace(JobTitle))
                    friendlyName = $"{JobTitle} {friendlyName}";

                return friendlyName;
            }
        }
        public string JobTitle { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string Configuration { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLockedOut => this.LockoutEnabled && this.LockoutEnd >= DateTimeOffset.UtcNow;
        public int GroupId { get; set; }                                                            //This field is a Group Model's foreign Key.
        public int? DepartmentId { get; set; }                                                      //This is a Department Model's foreign key.
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string PictureFile { get; set; }
        public DateTime LastActivity { get; set; }                                                  // Date and time of last activity
        public string LastText { get; set; }                                                        // Last text send or receaved
        public string WebPushId { get; set; }                                                       // webpush subscriber Id
        public NotificationLevel Notification { get; set; }
        public DateTime LastContactTransfered { get; set; }                                         // Data hora em que o Bot transferiu o ultimo contato para o atendente - para fazer circular uma chamada para cada atendente

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Roles { get; }

        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; }


        // Related Models
        public virtual ICollection<ChattingLog> ChatMessages { get; }       // list of all Messages sent or receaved by this Agent
        public virtual ICollection<Contact> Contacts { get; }               // list of all Contacts assigned to this Agent
        public virtual ICollection<Board> Boards { get; }                   // list of all Boards assigned to this Agent
        public virtual ICollection<Filter> Filters { get; }                 // list of all Filters assigned to this Agent
    }
}
