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

namespace ContactCenter.Infrastructure.Clients.GsWhatsApp
{
	// GsApi
	// Classe para fazer as chamadas da API GupShup para envio de mensagems para o WhatsApp
	public class GsWhatsAppClient
	{

		// Variaveis de configuração para usar a API
		private readonly string gsApiKey;
		private readonly Uri gsApiUri;
		private readonly Uri gsMediaUri;

		// Constructor
		public GsWhatsAppClient(IOptions<GsWhatsAppSettings> gsSettings)
		{
			if (gsSettings == null)
				throw new ArgumentException("Argument missing:", nameof(gsSettings));

			gsApiKey = gsSettings.Value.GsApiKey;
			gsApiUri = gsSettings.Value.GsApiUri;
			gsMediaUri = gsSettings.Value.GsMediaUri;
		}

		// Tipos de midia suportados
		public enum Mediatype
		{
			image,
			audio,
			file,
			video,
		}
		public async Task<string> SendMedia(string whatsAppNumber, string destination, Mediatype mediatype, string filename, Uri contentUri, [Optional] Uri thumbnailUri, [Optional] string apiKey)
		{

			if (contentUri == null)
				throw new ArgumentException("Argument missing:", nameof(contentUri));

			// If apiKey was not provided, uses default
			if (string.IsNullOrEmpty(apiKey))
				apiKey = gsApiKey;

			HttpClient httpClient = new HttpClient();
			try
			{
				// Cabeçalhos Http
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				// Monta o corpo da requisição
				string content = "channel=whatsapp&source=" + whatsAppNumber + "&destination=" + destination + "&message.payload={\"type\":\"" + mediatype.ToString() + "\",";
				if (mediatype != Mediatype.audio)
					content += "\"filename\":\"" + filename + "\",";
				if (mediatype == Mediatype.image)
				{
					content += "\"originalUrl\":\"" + contentUri + "\"";
					if (thumbnailUri != null)
						content += ",\"previewUrl\":\"" + thumbnailUri + "\"";
					else
						content += ",\"previewUrl\":\"" + contentUri + "\"";

					content += "}";
				}
				else
					content += "\"url\":\"" + contentUri.ToString() + "\"}";

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				// Faz a requisição e lê a resposta de forma assíncrona
				var httpResponseMessage = await httpClient.PostAsync(gsApiUri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
				httpContent.Dispose();
				httpClient.Dispose();

				// Desserializa o objeto mensagem
				GsReturn gsReturn = JsonConvert.DeserializeObject<GsReturn>(resp);

				// Devolve o Id da mensagem
				return gsReturn.MessageId;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendImages: " + ex);
				httpClient.Dispose();
				return string.Empty;
			}

		}

		public async Task<string> SendText(string whatsAppNumber, string destination, string text, [Optional] string apiKey)
		{

			// If apiKey was not provided, uses default
			if (string.IsNullOrEmpty(apiKey))
				apiKey = gsApiKey;

			text = WebUtility.UrlEncode(text);

			HttpClient httpClient = new HttpClient();
			try
			{
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				string content = "channel=whatsapp&source=" + whatsAppNumber + "&destination=" + destination + "&message=" + text;

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);

				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				var httpResponseMessage = await httpClient.PostAsync(gsApiUri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

				httpContent.Dispose();
				httpClient.Dispose();

				// Desserializa o objeto mensagem
				GsReturn gsReturn = JsonConvert.DeserializeObject<GsReturn>(resp);

				// Devolve o Id da mensagem
				return gsReturn.MessageId;

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				httpClient.Dispose();
				return string.Empty;
			}

		}
		public async Task<string> SendInteractiveMessage(string whatsAppNumber, string destination, string payload, [Optional] string apiKey)
		{

			// If apiKey was not provided, uses default
			if (string.IsNullOrEmpty(apiKey))
				apiKey = gsApiKey;

			HttpClient httpClient = new HttpClient();
			try
			{
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				string content = "channel=whatsapp&source=" + whatsAppNumber + "&destination=" + destination + "&message=" + payload;

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);

				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				var httpResponseMessage = await httpClient.PostAsync(gsApiUri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

				httpContent.Dispose();
				httpClient.Dispose();

				// Desserializa o objeto mensagem
				GsReturn gsReturn = JsonConvert.DeserializeObject<GsReturn>(resp);

				// Devolve o Id da mensagem
				return gsReturn.MessageId;

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				httpClient.Dispose();
				return string.Empty;
			}

		}

		public async Task<bool> OptIn(string appName, string customerNumber, [Optional] string apiKey)
		{

			// If apiKey was not provided, uses default
			if (string.IsNullOrEmpty(apiKey))
				apiKey = gsApiKey;

			HttpClient httpClient = new HttpClient();
			try
			{
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				string content = "user=" + customerNumber;

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);

				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				Uri gsOptInUri = new Uri($"https://api.gupshup.io/sm/api/v1/app/opt/in/{appName}");
				var httpResponseMessage = await httpClient.PostAsync(gsOptInUri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

				httpContent.Dispose();
				httpClient.Dispose();
				return true;

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				httpClient.Dispose();
				return false;
			}

		}
		public async Task<string> SendHsmText(string whatsAppNumber, string destination, string text, [Optional] string apiKey)
		{

			// If apiKey was not provided, uses default
			if (string.IsNullOrEmpty(apiKey))
				apiKey = gsApiKey;

			if (text.Contains("**"))
				text = text.Replace("**", "*");

			text = WebUtility.UrlEncode(text);

			HttpClient httpClient = new HttpClient();
			try
			{
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				string content = "channel=whatsapp&source=" + whatsAppNumber + "&destination=" + destination + "&message.payload={\"type\":\"text\",\"text\":\"" + text + "\", \"isHSM\":\"true\"}";

				HttpContent httpContent = new StringContent(content, Encoding.UTF8);

				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				var httpResponseMessage = await httpClient.PostAsync(gsApiUri, httpContent).ConfigureAwait(false);
				string resp = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

				httpContent.Dispose();
				httpClient.Dispose();

				// Desserializa o objeto mensagem
				GsReturn gsReturn = JsonConvert.DeserializeObject<GsReturn>(resp);

				// Devolve o Id da mensagem
				return gsReturn.MessageId;

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				httpClient.Dispose();
				return string.Empty;
			}

		}
		public async Task<Stream> GetVoice(string whatsAppAppname, string voiceID, [Optional] string apiKey)
		{
			try
			{

				// If apiKey was not provided, uses default
				if (string.IsNullOrEmpty(apiKey))
					apiKey = gsApiKey;

				string uri = gsMediaUri + whatsAppAppname + "/" + voiceID;

				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				using var request = new HttpRequestMessage(HttpMethod.Get, uri);
				Stream contentStream = await (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false);

				request.Dispose();
				httpClient.Dispose();
				return (contentStream);

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				Stream stream = new MemoryStream();
				return (stream);
			}
		}
		public async Task<Stream> GetAudio(Uri uri, [Optional] string apiKey)
		{
			try
			{

				// If apiKey was not provided, uses default
				if (string.IsNullOrEmpty(apiKey))
					apiKey = gsApiKey;

				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", apiKey);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				using var request = new HttpRequestMessage(HttpMethod.Get, uri);
				Stream contentStream = await (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false);

				request.Dispose();
				httpClient.Dispose();
				return (contentStream);

			}
			catch (Exception ex)
			{
				Debug.WriteLine("GsApi: SendText: " + ex);
				Stream stream = new MemoryStream();
				return (stream);
			}
		}

	}
	public class GsReturn
	{
		[JsonProperty("status")]
		internal string Status { get; set; }
		[JsonProperty("messageId")]
		internal string MessageId { get; set; }
	}
}
