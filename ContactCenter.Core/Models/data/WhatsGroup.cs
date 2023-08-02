using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactCenter.Core.Models
{
	// Representa um grupo de whats app
	public class WhatsGroup
	{
		public int Id { get; set; }
		public int GroupCampaignId { get; set; }
		[StringLength(256)]
		public string Wid { get; set; }
		[StringLength(256)]
		public string InviteUrl { get; set; }
		public int Clicks { get; set; }
		public int Leads { get; set; }
		public int Members { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool Created { get; set; }
		public bool ImageSet { get; set; }
		public bool DescriptionSet { get; set; }
		public int Number { get; set; }
		[StringLength(256)]
		public string Obs { get; set; }
		[StringLength(256)]
		public string Name { get; set; }
		public int CreateErrorCount { get; set; }
		public int SetImageErrorCount { get; set; }
		public bool GroupWelcomeMessageSent { get; set; }
	}
}
