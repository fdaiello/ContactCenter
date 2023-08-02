using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ContactCenter.Core.Models
{
	/*
	 * Campanha de lançamento em grupos
	 * Define um link único que redericionará para links de grupos no whats app
	 */
	public class GroupCampaignDto
	{
		public GroupCampaignDto(GroupCampaign groupCampaign)
		{
			if (groupCampaign != null)
			{
				foreach (PropertyInfo property in typeof(GroupCampaignDto).GetProperties())
				{
					if ( property.Name != nameof(this.WhatsGroupsCount)) { 
						var x = groupCampaign.GetType().GetProperty(property.Name).GetValue(groupCampaign, null);
						property.SetValue(this, x, null);
					}
				}

				if (groupCampaign.WhatsGroups != null)
					this.WhatsGroupsCount = groupCampaign.WhatsGroups.Where(p => p.Created).Count();
				else
					this.WhatsGroupsCount = 0;

			}
		}
		public int Id { get; set; }                                 // Primary Key
		public virtual Group Group { get; set; }
		public string Name { get; set; }                            // Campaign Name
		public string Description { get; set; }                     // Campaign Description

		public string ImageFileName { get; set; }                   // Campaign Image file name
		public DateTime? CreatedDate { get; set; }
		public DateTime? EndDate { get; set; }						// Optional end date
		public string FacePixelCode { get; set; }
		public string GoogleAdsCode { get; set; }
		public string ClosedUrl { get; set; }
		public int Clicks { get; set; }
		public int Leads { get; set; }
		public int Members { get; set; }
		public int Groups { get; set; }
		public string LinkSufix { get; set; }							// Will be used to create smart page URL of enter link
		public int MessageId { get; set; }							    // Id da Smart Page que fará o redirecionamento para os grupos - e será a url de entrada
		public virtual Message Message { get; set; }
		public string ChatChannelId { get; set; }
		public virtual ChatChannel ChatChannel { get; set; }
		public int? GroupBoardId { get; set; }
		public virtual Board GroupBoard { get; set; }
		public int? LeadsListId { get; set; }
		public virtual Board LeadsBoard { get; set; }
		public int MaxClicksPerGroup { get; set; }                      // Max number of clicks that will be redirected to a single group
		public string GroupAdmins { get; set; }                         // String with wahts app group admins phone. May contain more than one phone number comma separted
		public GroupCampaingStatus Status { get; set; }
		public GroupCampaingPermissions Permissions { get; set; }       // Indicates who can send messages to group: all members, or admin only
		public string Obs { get; set; }
		public int? WelcomeMessageId { get; set; }                      // Indicates message that should be sent to user when it enters a group - message sent to user
		public int? GroupWelcomeMessageId { get; set; }                 // Indicates message that should be sent to whole group when it is full - message sent to group
		public int? LeaveMessageId { get; set; }                        // Indicates a message that should be sent to group member when it enters a group
		public string SendMsgChatChannelId { get; set; }
		public int WhatsGroupsCount { get; set; }
		public int FirstGroupNumber { get; set; }
		public int ErrorsCount 	{ get; set; }
		public GroupAction GroupAction { get; set; }
		public virtual ICollection<WhatsGroup> WhatsGroups { get; set; }
	}
}
