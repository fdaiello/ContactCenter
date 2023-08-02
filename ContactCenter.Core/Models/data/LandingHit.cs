using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
	public enum LandingHitType
	{
		get,
		post
	}
	public class LandingHit
	{
		public int Id { get; set; }
		public int LandingId { get; set; }
		public DateTime Time { get; set; }
		public LandingHitType HitType { get; set; }
		public string ContactId { get; set; }
		public virtual Landing Landing { get; set; }
		public virtual Contact Contact { get; set; }

	}
}
