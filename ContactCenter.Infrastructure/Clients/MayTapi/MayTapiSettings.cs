using System;

namespace ContactCenter.Infrastructure.Clients.MayTapi
{
	/// <summary>
	/// Defines values that a <see cref="MayTapiClient"/> can use to connect to WhatsApp's MayTapi API.
	/// </summary>
	public class MayTapiSettings
	{
		public Uri ApiUri { get; set; }
		public string ProductId { get; set; }
		public string Token { get; set; }
	}
}
