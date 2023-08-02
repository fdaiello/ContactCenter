using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactCenter.Core.Models
{
	public class ExternalAccount
	{
		public Guid Id { get; set; }
		public string ContactId { get; set; }
		public Contact Contact { get; set; }
		public bool Autenticated { get; set; }
		public bool Selected { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public Guid UserId { get; set; }
		public ExternalAccount ShallowCopy()
		{
			return (ExternalAccount)this.MemberwiseClone();
		}
	}
}
