using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ContactCenter.Infrastructure.Clients.Wpush
{
	// WebPushR API Client
	// Classe para enviar notificações WebPush - usando www.wepbushr.com API
	public class WpushClient
	{
		// Variaveis lidas de AppSettings
		private readonly string key;
		private readonly string token;
		private readonly string apiurl;
		private readonly string icon;

		// Constructor
		public WpushClient(IOptions<WpushSettings> wpushSettings)
		{
			key = wpushSettings.Value.Key;
			token = wpushSettings.Value.Token;
			apiurl = wpushSettings.Value.ApiUrl;
			icon = wpushSettings.Value.Icon;
		}

		public async Task<Boolean> SendNotification(string title, string message, string targeturl, string sid = "")
		{

			message = string.IsNullOrEmpty(message) ? string.Empty : message.Replace("\n", "<br />", StringComparison.OrdinalIgnoreCase);

			HttpClient httpClient = new HttpClient();
			try
			{
				// Cabeçalhos Http
				httpClient.DefaultRequestHeaders.Add("User-Agent", "WebPushApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("webpushrKey", key);
				httpClient.DefaultRequestHeaders.Add("webpushrAuthToken", token);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				// Monta o corpo da requisição
				string content = @"{""title"":""" + title + @""",""message"":""" + message + @""",""target_url"":""" + targeturl + @"""";
				if (!string.IsNullOrEmpty(sid))
					content += @", ""sid"":""" + sid + @"""";
				content += @", ""icon"":""" + icon + @"""}";

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				// Monta a uri - com base na url de requisição - dependendo se a notificação é para todos ou para um unico subscriber
				Uri apiuri;
				if (string.IsNullOrEmpty(sid))
					apiuri = new Uri(apiurl + "/all");
				else
					apiuri = new Uri(apiurl + "/sid");

				// Faz a requisição e lê a resposta de forma assíncrona
				var httpResponseMessage = await httpClient.PostAsync(apiuri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

				httpContent.Dispose();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("WpushApi: SendAll: " + ex);
				httpClient.Dispose();
				return false;
			}

			httpClient.Dispose();
			return true;
		}
	}
}
