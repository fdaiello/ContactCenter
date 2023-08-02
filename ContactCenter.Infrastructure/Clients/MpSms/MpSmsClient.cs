using Newtonsoft.Json;
using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ContactCenter.Infrastructure.Clients.MpSms
{
	// MpSmsClient
	// Classe para fazer as chamadas da API da Mister Postman para envio de SMS
	public class MpSmsClient
	{

		// Variaveis de configuração para usar a API
		private readonly Uri mpApiUri;
		private readonly string mpUserId;
		private readonly string mpToken;

		// Injected Logger
		private readonly ILogger _logger;

		// Constructor
		public MpSmsClient(IOptions<MpSmsSettings> mpSmsSettings, ILogger<MpSmsClient> logger)
		{
			if (mpSmsSettings == null)
				throw new ArgumentException("Argument missing:", nameof(mpSmsSettings));

			mpApiUri = mpSmsSettings.Value.ApiUri;
			mpUserId = mpSmsSettings.Value.UserId;
			mpToken = mpSmsSettings.Value.Token;

			_logger = logger;
		}


		public async Task<string> SendSms(string description, string destination, string text, string UserId = null, string Token = null)
		{

			// Codifica parâmetros para uso em URL
			text = WebUtility.UrlEncode(text);
			description = WebUtility.UrlEncode(description);

			// Trunca mensagens maiores que 304 caracteres
			if ( text.Length > 304 )
			{
				text = text.Substring(0, 304);
			}

			// If UserId and Token were not passed, uses default configuration
			if (UserId == null)
				UserId = mpUserId;
			if (Token == null)
				Token = mpToken;

			HttpClient httpClient = new HttpClient();
			try
			{
				Uri mpApiUriGet = new Uri(mpApiUri + "?UserId=" + UserId + "&Token=" + Token + "&descricao=" + description + "&NroDestino=" + destination + "&Mensagem=" + text + "&ID=true");


				var httpResponseMessage = await httpClient.GetAsync(mpApiUriGet).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
				httpClient.Dispose();

				// Verifica se voltou o Ok
				string messageId = string.Empty;
				if (resp.Contains("OK;"))
				{
					messageId = resp.Split("OK;")[1].Trim();
				}

				// Devolve o Id da mensagem
				return messageId;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException.Message);

				httpClient.Dispose();
				return string.Empty;
			}

		}

	}
}
