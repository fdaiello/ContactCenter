using System;

namespace ContactCenter.Infrastructure.Clients.Wassenger
{
	/// <summary>
	/// Defines values that a <see cref="WassengerClient"/> can use to connect to WhatsApp's Wassenger API.
	/// </summary>
	public class WassengerSettings
	{
		public Uri ApiUri { get; set; }

		public string Token { get; set; }
	}
}
