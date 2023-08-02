using System;

namespace ContactCenter.Infrastructure.Clients.MpSms
{
	/// <summary>
	/// Defines values that a <see cref="MpSmsClient"/> can use to send SMS using Mister Postman API
	/// </summary>
	public class MpSmsSettings
	{

		/// <summary>
		/// Gets or sets the URI for the API calls
		/// </summary>
		public Uri ApiUri { get; set; }

		/// <summary>
		/// Gets or sets UserId from Mister Postman account.
		/// </summary>
		/// <value>The account UserId.</value>
		public string UserId { get; set; }

		/// <summary>
		/// Gets or sets the Token ( like password ) from the Mister Postman account
		/// </summary>
		public string Token { get; set; }
	}
}
