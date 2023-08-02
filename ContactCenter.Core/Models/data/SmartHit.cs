using System;
using System.Collections.Generic;
using System.Text;

namespace ContactCenter.Core.Models
{
	public class SmartHit
	{
		public int Id { get; set; }
		public int MessageId { get; set; }
		public int? SendingId { get; set; }
		public DateTime Time { get; set; }
	}
}
