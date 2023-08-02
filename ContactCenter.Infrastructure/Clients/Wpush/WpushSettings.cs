using Microsoft.Extensions.Options;

namespace ContactCenter.Infrastructure.Clients.Wpush
{
	public class WpushSettings
	{
		public string ApiUrl { get; set; }
		public string Key { get; set; }
		public string Token { get; set; }
		public string Icon { get; set; }
	}
}
