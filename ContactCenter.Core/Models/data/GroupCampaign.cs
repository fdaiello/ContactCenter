
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ContactCenter.Core.Models
{
	public enum GroupCampaingStatus
    {
		initializing,
		active,
		closed,
		error
    }
	public enum GroupCampaingPermissions
    {
		admins,
		all
    }
	public enum GroupAction
	{
		create,
		import
	}
	/*
	 * Campanha de lançamento em grupos
	 * Define um link único que redericionará para links de grupos no whats app
	 */
	public class GroupCampaign
	{
		public int Id { get; set; }                                 // Primary Key
		public int? GroupId { get; set; }                           // Foreing Key to the Group/Company that this campaign belongs
		public virtual Group Group { get; set; }
		[StringLength(256)]
		public string Name { get; set; }							// Campaign Name
		[StringLength(2048)]
		public string Description { get; set; }                     // Campaign Description
		[StringLength(256)]
		public string ImageFileName { get; set; }					// Campaign Image file name
		public DateTime? CreatedDate { get; set; }
		public DateTime? EndDate { get; set; }						// Optional end date
		[StringLength(128)]
		public string FacePixelCode { get; set; }
		[StringLength(256)]
		public string GoogleAdsCode { get; set; }
		[StringLength(256)]
		public string ClosedUrl { get; set; }
		public int Clicks { get; set; }
		public int Leads { get; set; }
		public int Members { get; set; }
		public int Groups { get; set; }
		[StringLength(64)]
		public string LinkSufix { get; set; }				      		// Will be used to create smart page URL of enter link
		public int MessageId { get; set; }							    // Id da Smart Page que fará o redirecionamento para os grupos - e será a url de entrada
		public virtual Message Message { get; set; }
		[StringLength(32)]
		public string ChatChannelId { get; set; }
		public virtual ChatChannel ChatChannel { get; set; }
		public int? GroupBoardId { get; set; }
		[ForeignKey("GroupBoardId")]
		public virtual Board GroupBoard { get; set; }
		public int? LeadsListId { get; set; }
		[ForeignKey("LeadsListId")]
		public virtual Board LeadsBoard { get; set; }
		public int MaxClicksPerGroup { get; set; }                      // Max number of clicks that will be redirected to a single group
		[StringLength(64)]
		public string GroupAdmins { get; set; }							// String with whats app group admins phone. May contain more than one phone number comma separted
		public GroupCampaingStatus Status { get; set; }
		[StringLength(1024)]
		public string Obs { get; set; }
		public GroupCampaingPermissions Permissions { get; set; }		// Indicates who can send messages to group: all members, or admin only
		public int? WelcomeMessageId { get; set; }						// Indicates message that should be sent to user when it enters a group - individual message - sent directly to user
		[ForeignKey("WelcomeMessageId")]
		public virtual Message WelcomeMessage { get; set; }
		public int? GroupWelcomeMessageId { get; set; }                 // Indicates message that should be sent to whole group when it is full
		[ForeignKey("GroupWelcomeMessageId")]
		public virtual Message GroupWelcomeMessage { get; set; }
		public int? LeaveMessageId { get; set; }                        // Indicates a message that should be sent to group member when it leaves a group
		[ForeignKey("LeaveMessageId")]
		public virtual Message LeaveMessage { get; set; }               
		public string SendMsgChatChannelId { get; set; }
		[ForeignKey("SendMsgChatChannelId")]
		public virtual ChatChannel SendMsgChatChannel { get; set; }
		public virtual ICollection<WhatsGroup> WhatsGroups { get; set;  }

		public bool Updating { get; set; }
		public bool ChangeImage { get; set; }
		public bool ChangePermissions { get; set; }
		public bool ChangeAdmins { get; set; }
		public bool ChangeDescription { get; set; }
		public int FirstGroupNumber { get; set; }
		public int ErrorsCount { get; set; }
		public GroupAction GroupAction { get; set; }

	}
}
