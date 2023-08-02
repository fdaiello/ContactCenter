using System;

namespace ContactCenter.Infrastructure.Clients.GsWhatsApp
{
	/// <summary>
	/// Defines values that a <see cref="GsWhatsAppClientWrapper"/> can use to connect to WhatsApp's using GupShup API.
	/// </summary>
	public class GsWhatsAppSettings
	{

		/// <summary>
		/// Gets or sets API KEY from the GupShup account.
		/// </summary>
		/// <value>The account SID.</value>
		public string GsApiKey { get; set; }

		/// <summary>
		/// Gets or sets the URI for the API calls
		/// </summary>
		public Uri GsApiUri { get; set; }

		/// <summary>
		/// Gets or sets the URL for getting media files
		/// </summary>
		public Uri GsMediaUri { get; set; }
	}
}
