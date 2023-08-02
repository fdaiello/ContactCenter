using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Data;
using ContactCenter.Core.Models;
using ContactCenter.Infrastructure.Utilities;
using System.Net;

namespace ContactCenter.Infrastructure.Clients.Wassenger
{
	// Classe para fazer as chamadas da API Wassenger para envio de mensagems para o WhatsApp
	public class WassengerClient
	{

		// Variaveis de configuração para usar a API
		private readonly Uri apiUri;
		private readonly string token;

		// Injected logger and configuration
		private readonly ILogger _logger;
		private readonly DbContextOptionsBuilder<ApplicationDbContext> _dbOptionsBuilder;

		// Dictionary to save scanned phones info
		private readonly IDictionary<string, int> scannedPhones;

		// Constructor
		public WassengerClient(IOptions<WassengerSettings> waSettings, ILogger<WassengerClient> logger, DbContextOptionsBuilder<ApplicationDbContext> dbOptionsBuilder)
		{
			// Check if parameters were sent
			if (waSettings == null)
				throw new ArgumentException("Argument missing:", nameof(waSettings));

			// Configure client settings
			token = waSettings.Value.Token;
			apiUri = waSettings.Value.ApiUri;

			// Get injected objects
			_logger = logger;
			_dbOptionsBuilder = dbOptionsBuilder;

			// Initialize Dictionary
			scannedPhones = new Dictionary<string, int>();
		}

		// Tipos de midia suportados
		public enum Mediatype
		{
			image,
			audio,
			file,
			video,
		}
		private async Task<string> UploadFile(Mediatype mediatype, string filename, Uri contentUri)
		{
			string fileId = string.Empty;

			try
			{
				if (mediatype == Mediatype.audio || mediatype == Mediatype.video)
				{
					// Upload an audio file to be sent as non-voice recorded audio format
					var client = new RestClient("https://api.wassenger.com/v1/files");
					var request = new RestRequest(Method.POST);
					request.AddHeader("content-type", "application/json");
					request.AddHeader("token", token);
					request.AddParameter("application/json", "{\"url\":\"" + contentUri.ToString() + "\",\"format\":\"native\"}", ParameterType.RequestBody);
					IRestResponse response = client.Execute(request);

					// Confere se a resposta é objeto simples Json ou Array
					if (response.Content.StartsWith("["))
						// Remove os colchetes
						response.Content = response.Content.Replace("[", "").Replace("]", "");

					// Desserializa o conteudo do retorno
					WaReturn waReturn = JsonConvert.DeserializeObject<WaReturn>(response.Content);

					// Confere se voltou Id
					if (waReturn.id != null)
						fileId = waReturn.id;
					else if (waReturn.message.StartsWith("File already exists with ID "))
						fileId = waReturn.message.Split(".")[0].Replace("File already exists with ID ", "");
					else
					{
						_logger.LogError("Wassenger.WassengerClient.UploadFile. No FileId was returned." + response.Content);
						fileId = string.Empty;
					}
				}
				else
				{
					// Upload file and get id
					var client = new RestClient("https://api.wassenger.com/v1/files?reference=" + filename);
					var request = new RestRequest(Method.POST);
					request.AddHeader("content-type", "application/json");
					request.AddHeader("token", token);
					request.AddParameter("application/json", "{\"url\":\"" + contentUri.ToString() + "\"}", ParameterType.RequestBody);
					IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

					// Confere se a resposta é objeto simples Json ou Array
					if (response.Content.StartsWith("["))
						// Remove os colchetes
						response.Content = response.Content.Replace("[", "").Replace("]", "");

					// Desserializa o conteudo do retorno
					WaReturn waReturn = JsonConvert.DeserializeObject<WaReturn>(response.Content);

					// Confere se voltou Id
					if (waReturn.id != null)
						fileId = waReturn.id;
					else if (waReturn.message.StartsWith("File already exists with ID "))
						fileId = waReturn.message.Split(".")[0].Replace("File already exists with ID ", "");
					else
					{
						Console.WriteLine("Wassenger.WassengerClient.UploadFile. No FileId was returned." + response.Content);
						fileId = string.Empty;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Wassenger.WassengerClient.UploadFile: " + ex.Message);
				fileId = string.Empty;
			}

			return fileId;
		}
		public async Task<string> SendMediaMessage(string deviceid, string destination, Mediatype mediatype, string filename, Uri contentUri, string text = "")
		{

			if (contentUri == null)
				throw new ArgumentException("Argument missing:", nameof(contentUri));

			if (destination == null)
				throw new ArgumentException("Argument missing:", nameof(destination));

			// Insert plus at internationa numbers, if there is not already
			if (!destination.StartsWith("+") && !destination.Contains("@"))
				destination = "+" + destination;

			// Line Break
			text = text.Replace("\n", @"\n").Replace("\r", "");

			try
			{
				// Upload file and get id
				string fileId = await UploadFile(mediatype, filename, contentUri).ConfigureAwait(false);

				// Espera - nao ta funcionando se enviar logo em seguida
				Thread.Sleep(2000);

				if (!string.IsNullOrEmpty(fileId))
				{
					// Send media message to user. Note the file must be updated first, see API endpoint: Files > Upload file
					var client2 = new RestClient("https://api.wassenger.com/v1/messages");
					var request2 = new RestRequest(Method.POST);
					string format = "native";
					request2.AddHeader("content-type", "application/json");
					request2.AddHeader("token", token);
					string jsonPayload;

					if ( destination.Contains("@"))
						jsonPayload = "{\"device\":\"" + deviceid + "\",\"group\":\"" + destination + "\",\"message\":\"" + text + "\",\"media\":{\"file\":\"" + fileId + "\",\"format\":\"" + format + "\"}}";

					else
						jsonPayload = "{\"device\":\"" + deviceid + "\",\"phone\":\"" + destination + "\",\"message\":\"" + text + "\",\"media\":{\"file\":\"" + fileId + "\",\"format\":\"" + format + "\"}}";

					request2.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
					IRestResponse response2 = await client2.ExecuteAsync(request2).ConfigureAwait(false);

					// Desserializa o conteudo do retorno
					WaReturn waReturn2 = JsonConvert.DeserializeObject<WaReturn>(response2.Content);

					// Devolve o Id da mensagem
					if (waReturn2.id == null)
					{
						_logger.LogError("Wassenger.WassengerClient.SendMedia: No Id:" + response2.Content);
						return string.Empty;
					}
					else
					{
						_logger.LogInformation($"Wassenger.WassengerClient.SendMedia. Destination:{destination}, fileId:{fileId}");
						return waReturn2.id;
					}
				}
				else
				{
					_logger.LogError("Wassenger.WassengerClient.SendMedia: No FileId was returned.");
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Wassenger.WassengerClient.SendMedia: " + ex.Message);
				return string.Empty;
			}

		}
		public async Task<string> SendMessage(string deviceid, string destination, string text)
		{

			// Bold sign
			if (text.Contains("**"))
				text = text.Replace("**", "*");

			// Insert plus at internationa numbers, if there is not already
			if (!destination.StartsWith("+") && !destination.Contains("@"))
				destination = "+" + destination;

			// Line Break
			text = text.Replace("\n", @"\n").Replace("\r", "");

			try
			{
				// Send text message that should be delivered now
				var client = new RestClient("https://api.wassenger.com/v1/messages");
				var request = new RestRequest(Method.POST);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				string jsonPayload = string.Empty;
				if (destination.Contains("@"))
					jsonPayload = "{\"device\":\"" + deviceid + "\",\"group\":\"" + destination + "\",\"message\":\"" + text + "\"}"; 
				else
					jsonPayload = "{\"device\":\"" + deviceid + "\",\"phone\":\"" + destination + "\",\"message\":\"" + text + "\"}";

				request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Desserializa o objeto mensagem
				WaReturn waReturn = JsonConvert.DeserializeObject<WaReturn>(response.Content);

				// Log Information
				_logger.LogInformation($"SendMessage. destination:{destination}, text: {text}");

				// Devolve o Id da mensagem
				return waReturn.id;

			}
			catch (Exception ex)
			{
				_logger.LogError("Wassenger.WassengerClient.SendMessage: " + ex.Message);
				return string.Empty;
			}

		}
		public async Task<Stream> GetAudio(Uri uri)
		{
			try
			{

				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("User-Agent", "GsApi/1.0");
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				httpClient.DefaultRequestHeaders.Add("Apikey", token);
				httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

				using var request = new HttpRequestMessage(HttpMethod.Get, uri);
				Stream contentStream = await (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false);

				request.Dispose();
				httpClient.Dispose();
				return (contentStream);

			}
			catch (Exception ex)
			{
				_logger.LogError("Wassenger.WassengerClient.GetAudio: " + ex.Message);
				Stream stream = new MemoryStream();
				return (stream);
			}
		}
		public async Task<List<WaDevice>> GetDevices()
        {

			try
            {
				// Get account devices
				var client = new RestClient("https://api.wassenger.com/v1/devices");
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Desserializa o retorno
				List<WaDevice> devices = JsonConvert.DeserializeObject<List<WaDevice>>(response.Content);

				return (devices);

			}
			catch ( Exception ex )
            {
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);
				return new List<WaDevice>();
            }

		}
		public async Task<WaDevice> GetDevice(string deviceId)
		{

			try
			{
				// Get account devices
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}");
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Desserializa o retorno
				WaDevice device = JsonConvert.DeserializeObject<WaDevice>(response.Content);

				return (device);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);
				return new WaDevice();
			}

		}
		public async Task<string> ScanDevice(string deviceId)
		{

			string content = string.Empty;

			try
			{
				// Get account devices
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}/scan");
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Check if response is QRCode or JSon with error msg
				if (response.Content.StartsWith("{"))
				{
					// Get Error Message
					ScanDeviceResult scanDeviceResult = JsonConvert.DeserializeObject<ScanDeviceResult>(response.Content);
					content = scanDeviceResult.message;
				}
				else
                {
					// Get QRCode: image/xml+svg
					content = response.Content;
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);
			}

			return (content);

		}
		/// <summary>
		/// Get lists of contacts of given deviceId
		/// </summary>
		/// <param name="deviceId">Id of device - attached to some WhatsAppNumber</param>
		/// <param name="size">max number of contacts to read in one call</param>
		/// <param name="page">page number - so that we can get more contacts in another call</param>
		/// <returns>List of Contacts</returns>
		public async Task<List<WaContact>> GetContats(string deviceId, [Optional] int size, [Optional] int page)
		{

			try
			{
				// default quantity of contacts to return on a single call
				if (size == 0) size = 20;

				// url to get contacts
				string apiUrl = $"https://api.wassenger.com/v1/io/{deviceId}/contacts?size={size}&page={page}";

				// GET Url with token at the Header
				var client = new RestClient(apiUrl);
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Desserializa o retorno
				List<WaContact> waContacts = JsonConvert.DeserializeObject<List<WaContact>>(response.Content);

				// Retorna a lista de contatos
				return (waContacts);

			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

				// devolve lista vazia
				return new List<WaContact>();
			}

		}
		/// <summary>
		/// Get a contact with all details
		/// </summary>
		/// <param name="deviceId">Id of device - attached to some WhatsAppNumber</param>
		/// <param name="contactWid">WhatsApp Id of contact</param>
		/// <returns>WaContact</returns>
		public async Task<WaContact> GetContatDetail(string deviceId, string contactWid)
		{

			try
			{

				// url to get contacts
				string apiUrl = $"https://api.wassenger.com/v1/io/{deviceId}/contacts/{contactWid}";

				// GET Url with token at the Header
				var client = new RestClient(apiUrl);
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Desserializa o retorno
				WaContact waContact = JsonConvert.DeserializeObject<WaContact>(response.Content);

				// Retorna a lista de contatos
				return (waContact);

			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

				// devolve nulo
				return null;
			}

		}
		/*
		 * Sincroniza o chats do celular com a API
		 */
		public async Task<bool> SyncChats(string deviceId)
		{
			try
			{
				// url to get contacts
				string apiUrl = $"https://api.wassenger.com/v1/io/{deviceId}/sync";

				// GET Url with token at the Header
				var client = new RestClient(apiUrl);
				var request = new RestRequest(Method.POST);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				return response.StatusCode == HttpStatusCode.OK;

			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

				return false;
			}
		}
		public async Task Synchronize(string deviceId)
		{

			if (!string.IsNullOrEmpty(deviceId))
			{
				int page = 0;
				int size = 100;
				List<WaContact> waContacts = new List<WaContact>();
				string phone = string.Empty;
				string contactId;

				// Check if deviceId is not present at Dictionary
				if (!scannedPhones.ContainsKey(deviceId))
				{
					// Insert device Id at dictionary
					scannedPhones.Add(deviceId, 0);

					try
					{
						// Crate a new instace of DbContext
						ApplicationDbContext applicationDbContext = new ApplicationDbContext(_dbOptionsBuilder.Options);

						// Get device info from Db
						ChatChannel chatChannel = await applicationDbContext.ChatChannels.FindAsync(deviceId);
						Contact contact;

						if (chatChannel != null)
						{
							do
							{
								waContacts = await GetContats(deviceId, size, page).ConfigureAwait(false);

								foreach (WaContact waContact in waContacts.Where(p => p.groupId == null & !string.IsNullOrEmpty(p.name)))
								{

									// Save phone number from Id
									phone = Utility.ClearStringNumber(waContact.phone);

									// Gera o contact.Id
									contactId = chatChannel.GroupId + "-" + phone;

									// Para os numeros do Brasil, remove o 55 da frente
									if (phone.StartsWith("55") & phone.Length > 11)
										phone = phone.Substring(2);

									// Search contat at database
									contact = await applicationDbContext.Contacts.FindAsync(contactId);

									// If number is not alredy at database
									if (contact == null)
									{
										// Create new contact
										Contact newContact = new Contact { MobilePhone = phone, Name = waContact.name ?? string.Empty, FullName = waContact.name ?? string.Empty, Id = contactId, ApplicationUserId = chatChannel.ApplicationUserId, DepartmentId = chatChannel.DepartmentId, GroupId = chatChannel.GroupId, ChatChannelId = chatChannel.Id, ChannelType = chatChannel.ChannelType, LastText = string.Empty, FirstActivity = Utility.HoraLocal()};
										// Add to database
										await applicationDbContext.Contacts.AddAsync(newContact).ConfigureAwait(false);
										// Save changes
										await applicationDbContext.SaveChangesAsync().ConfigureAwait(false);
									}

									// Increase counter
									scannedPhones[deviceId]++;
								}
								page++;

							} while (waContacts.Any());

						}

						applicationDbContext.Dispose();
					}
					catch (Exception ex)
					{
						_logger.LogError(ex.Message);
					}
				}
			}
		}
		public int Synchronized(string deviceId)
		{
			// Check if deviceId is not present at Dictionary

			if (scannedPhones.ContainsKey(deviceId))
				// Return counter for device
				return scannedPhones[deviceId];
			else
				// Return flag
				return -1;
		}
		public async Task<string> GetAndSaveStatus(string deviceId)
		{
			string status = string.Empty;

			try
			{
				WaDevice device = await GetDevice(deviceId).ConfigureAwait(false);

				if (device?.session != null)
				{
					// Crate a new instace of DbContext
					ApplicationDbContext applicationDbContext = new ApplicationDbContext(_dbOptionsBuilder.Options);

					status = device.session.status;
					ChatChannel channel = applicationDbContext.ChatChannels.Find(deviceId);
					if (channel != null)
					{
						channel.Status = status;
						channel.PhoneNumber = device.phone;
						await applicationDbContext.SaveChangesAsync();
					}

					applicationDbContext.Dispose();
				}
			}
			catch (Exception ex)
			{
				status = ex.Message;
			}

			return status;
		}

		public async Task<string> CreateGroup(string deviceId, string name, string description, GroupCampaingPermissions groupCampaingPermissions, string[] adminPhones)
		{

			try
			{
				// Create permisson object
				Permissions permissions = new Permissions { Edit = GroupCampaingPermissions.admins.ToString(), Send = groupCampaingPermissions.ToString() };

				// Create participants object - based on adminphones - can be a single number o comma separated values
				List<Participant> participants = new List<Participant>();

				foreach (string adminPhone in adminPhones)
				{
					// Phone number should be at least 12 characteres. Ex: +555191096510
					if (adminPhone.Length > 12 && adminPhone.StartsWith("+"))
						participants.Add(new Participant { Admin = true, Phone = adminPhone });
				}

				// Check for valid adminphones
				if (participants.Count == 0)
					return "Erro: Não foi informado pelo menos um telefone válido";

				// Creates WaGroup object
				WaCreateGroupRequest waCreateGroupRequest = new WaCreateGroupRequest { Name = name, Description = description, Participants=participants, Permissions=permissions };

				// Creates api request
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}/groups");
				var request = new RestRequest(Method.POST);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				string jsonPayload = JsonConvert.SerializeObject(waCreateGroupRequest);
				request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Confere se veio resposta
				if (!String.IsNullOrEmpty(response.Content))
                {
					// Desserializa a resposta
					WaCreateGroupReturn waGroup = JsonConvert.DeserializeObject<WaCreateGroupReturn>(response.Content);

					// Verifica se deu sucesse, e retorna
					if (waGroup.Id != null)
						return waGroup.Id;
					else
					{
						// Devolve a(s) mensagen(s) de erro
						string errMsg = waGroup.Message;
						foreach (Error error in waGroup.Errors)
						{
							errMsg += "; " + error.Message;
						}
						return "Erro: " + errMsg;
					}
				}
				else
                {
					return "Erro: " + response.ErrorMessage ?? string.Empty;

				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return "Erro:" + ex.Message;
			}

		}
		public async Task<string> SetGroupImage(string deviceId, string groupId, string imageUrl)
		{

			try
			{
				// Creates api request
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}/groups/{groupId}/image");
				var request = new RestRequest(Method.PUT);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				string jsonPayload = "{\"url\": \"" + imageUrl + "\"}";
				request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Devolve a resposta
				if (response.Content != null && response.Content.ToString().Contains("true"))
					return response.Content.ToString();

				else if ( response.Content!=null && response.Content.ToString().Contains("status"))
				{
					// Desserializa a resposta
					ScanDeviceResult scanDeviceResult = JsonConvert.DeserializeObject<ScanDeviceResult>(response.Content);
					return "Erro: " + scanDeviceResult.message;
				}

				else
					return "Erro: " + response.ErrorMessage;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return "Erro: " + ex.Message;
			}

		}
		public async Task<string[]> ValidateNumbers(string deviceId, string country, int prefix, string[] numbers)
		{

			try
			{
				// Create object to send number validation request
				List<NumberToValidate> numbersToValidate = new List<NumberToValidate>();
				foreach (string number in numbers)
					numbersToValidate.Add(new NumberToValidate { Phone = number });
				ValidateNumbersRequest validateNumbersRequest = new ValidateNumbersRequest { Country = country, Prefix = prefix, Numbers = numbersToValidate };

				// Creates api request
				var client = new RestClient("https://api.wassenger.com/v1/numbers/validate");
				var request = new RestRequest(Method.POST);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				string jsonPayload = JsonConvert.SerializeObject(validateNumbersRequest);
				request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Valida a resposta
				if (!string.IsNullOrEmpty(response.Content))
                {
					// Desserializa a resposta
					ValidateNumbersResult validateNumbersResult = JsonConvert.DeserializeObject<ValidateNumbersResult>(response.Content);

					// Devolve array com os números válidos
					return validateNumbersResult.Numbers.Where(p => p.Valid).Select(p => p.Phone).ToArray();

				}
				else
					return null;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return null;
			}

		}
		public async Task<string> GetInviteUrl(string deviceId, string groupId)
		{

			try
			{
				// Creates api request
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}/groups/{groupId}/invite");
				var request = new RestRequest(Method.GET);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Valida a resposta
				if (!string.IsNullOrEmpty(response.Content))
				{
					// Desserializa a resposta
					GetInviteUrlReturn getInviteUrlReturn = JsonConvert.DeserializeObject<GetInviteUrlReturn>(response.Content);

					// Devolve array a URL
					return getInviteUrlReturn.Url;

				}
				else
					return "Erro: " + response.ErrorMessage;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return "Erro: " + ex.Message;
			}

		}
		/*
		 * Altera os dados de um grupo do whats app
		 */
		public async Task<string> PatchGroup(string deviceId, string groupId, string name, string description, GroupCampaingPermissions groupCampaingPermissions)
		{

			try
			{
				// Create permisson object
				Permissions permissions = new Permissions { Edit = GroupCampaingPermissions.admins.ToString(), Send = groupCampaingPermissions.ToString() };

				// Creates PatchGroupRequest
				PatchGroupRequest patchGroupRequest = new PatchGroupRequest { Name = name, Description = description, Permissions = permissions };

				// Creates api request
				var client = new RestClient($"https://api.wassenger.com/v1/devices/{deviceId}/groups/{groupId}");
				var request = new RestRequest(Method.PATCH);
				request.AddHeader("content-type", "application/json");
				request.AddHeader("token", token);

				string jsonPayload = JsonConvert.SerializeObject(patchGroupRequest);
				request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Confere se veio resposta
				if (!String.IsNullOrEmpty(response.Content))
				{
					// Confere se deu erro
					if (response.Content.Contains("errorCode"))
					{
						// Desserializa a resposta, e devolve a mensagem de erro
						WaCreateGroupReturn waCreateGroupReturn = JsonConvert.DeserializeObject<WaCreateGroupReturn>(response.Content);
						return "Erro: " + waCreateGroupReturn.Message;

					}
					else
						return "Ok";
				}
				else
				{
					return "Erro: " + response.ErrorMessage ?? string.Empty;

				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return "Erro:" + ex.Message;
			}

		}
		/*
		 * Busca os grupos do aparelho
		 *    Devolve lista com os detalhes dos grupos
		 *    Usa a API dos Chats com parametro para pegar os grupos - mais atualizada que a API dos grupos
		 */
		public async Task<List<WaGroupDetail>> GetGroupChats(string deviceId)
		{

			List<WaGroupDetail> waGroupDetails = new List<WaGroupDetail>();

			try
			{
				// url to query groups
				int page = 0;
				const int pageSize = 500;
				int groupCount = 0;

				// Loop de páginas
				do
				{
					string apiUrl = $"https://api.wassenger.com/v1/io/{deviceId}/chats?type=group&size={pageSize}&page={page}";

					// GET Url with token at the Header
					var client = new RestClient(apiUrl);
					var request = new RestRequest(Method.GET);
					request.AddHeader("token", token);

					IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

					// Confere se veio resposta
					if (!String.IsNullOrEmpty(response.Content))
					{
						// Desserializa a resposta
						List<WaChat> waChats = JsonConvert.DeserializeObject<List<WaChat>>(response.Content);
						groupCount = waChats.Count();

						// Alimenta a lista dos grupos
						foreach (WaChat waChat in waChats)
						{
							waGroupDetails.Add(new WaGroupDetail { Wid = waChat.id, Name = waChat.name, TotalParticipants = 0 });
						}
					}

					page++;

				} while (groupCount == pageSize && page < 10 );
			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

			}

			// devolve lista
			return waGroupDetails;
		}
		/*
		 * Busca os grupos
		 */
		public async Task<List<WaGroupDetail>> GetGroups(string deviceId)
		{

			int page = 0;
			int size = 500;
			try
			{
				// url to query groups
				string apiUrl = $"https://api.wassenger.com/v1/devices/{deviceId}/groups?page={page}&size={size}";

				// GET Url with token at the Header
				var client = new RestClient(apiUrl);
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

				// Confere se veio resposta
				if (!String.IsNullOrEmpty(response.Content))
				{
					// Desserializa a resposta
					List<WaGroupDetail> waGroups = new List<WaGroupDetail>();
					waGroups = JsonConvert.DeserializeObject<List<WaGroupDetail>>(response.Content);

					// Devolve a lista dos grupos
					return waGroups;

				}
			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

			}

			// devolve lista vazia
			return new List<WaGroupDetail>();
		}
		/*
		 * Busca os detalhes de um grupo - incluindo a lista de membros do grupo
		 */
		public async Task<List<ParticipantV2>> GetGroupParticipants(string deviceId, string wId)
		{

			try
			{
				// url to get contacts
				string apiUrl = $"https://api.wassenger.com/v1/devices/{deviceId}/groups/{wId}";

				// GET Url with token at the Header
				var client = new RestClient(apiUrl);
				var request = new RestRequest(Method.GET);
				request.AddHeader("token", token);
				IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);


				// Confere se veio resposta
				if (!String.IsNullOrEmpty(response.Content))
				{
					// Desserializa a resposta
					WaGroupDetail waGroupDetail = JsonConvert.DeserializeObject<WaGroupDetail>(response.Content);
					// Devolve a lista de membros
					return waGroupDetail.Participants;

				}
				else
				{
					// devolve lista vazia
					return new List<ParticipantV2>();
				}

			}
			catch (Exception ex)
			{
				// registra erro
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
					_logger.LogError(ex.InnerException?.Message);

				// devolve lista vazia
				return new List<ParticipantV2>();
			}

		}


		class NumberToValidate
		{
			[JsonProperty("phone")]
			public string Phone { get; set; }
		}

		class ValidateNumbersRequest
		{
			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("prefix")]
			public int Prefix { get; set; }

			[JsonProperty("numbers")]
			public List<NumberToValidate> Numbers { get; set; }
		}
		class Summary
		{
			[JsonProperty("total")]
			public int Total { get; set; }

			[JsonProperty("valid")]
			public int Valid { get; set; }

			[JsonProperty("invalid")]
			public int Invalid { get; set; }
		}

		class Formats
		{
			[JsonProperty("local")]
			public string Local { get; set; }

			[JsonProperty("nationaal")]
			public string Nationaal { get; set; }

			[JsonProperty("international")]
			public string International { get; set; }
		}

		class ValidatedNumbers
		{
			[JsonProperty("valid")]
			public bool Valid { get; set; }

			[JsonProperty("error")]
			public object Error { get; set; }

			[JsonProperty("input")]
			public string Input { get; set; }

			[JsonProperty("phone")]
			public string Phone { get; set; }

			[JsonProperty("wid")]
			public string Wid { get; set; }

			[JsonProperty("kind")]
			public string Kind { get; set; }

			[JsonProperty("country")]
			public string Country { get; set; }

			[JsonProperty("countryPrefix")]
			public int CountryPrefix { get; set; }

			[JsonProperty("formats")]
			public Formats Formats { get; set; }
		}

		class ValidateNumbersResult
		{
			[JsonProperty("summary")]
			public Summary Summary { get; set; }

			[JsonProperty("numbers")]
			public List<ValidatedNumbers> Numbers { get; set; }
		}

		class GetInviteUrlReturn
		{
			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("url")]
			public string Url { get; set; }
		}
		class PatchGroupRequest
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("permissions")]
			public Permissions Permissions { get; set; }
		}

		class WaReturn
		{
			public string id { get; set; }
			public string waId { get; set; }
			public string phone { get; set; }
			public string wid { get; set; }
			public string status { get; set; }
			public string deliveryStatus { get; set; }
			public DateTime createdAt { get; set; }
			public DateTime sentAt { get; set; }
			public DateTime deliverAt { get; set; }
			public DateTime processedAt { get; set; }
			public string message { get; set; }
			public string priority { get; set; }
			public string retentionPolicy { get; set; }
			public Retry retry { get; set; }
			public string webhookStatus { get; set; }
			public WaMedia2 media { get; set; }
			public string device { get; set; }
		}
		class DeliveryOptions
		{
			public bool previewUrl { get; set; }
		}

		class ScanDeviceResult
		{
			public int status { get; set; }
			public string message { get; set; }
			public string errorCode { get; set; }
			public List<object> errors { get; set; }
		}

		class Permissions
		{
			[JsonProperty("edit")]
			public string Edit { get; set; }

			[JsonProperty("send")]
			public string Send { get; set; }
		}

		class WaCreateGroupRequest
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("permissions")]
			public Permissions Permissions { get; set; }

			[JsonProperty("participants")]
			public List<Participant> Participants { get; set; }
		}
		class Metadata
		{
			[JsonProperty("restrict")]
			public bool Restrict { get; set; }

			[JsonProperty("announce")]
			public bool Announce { get; set; }

			[JsonProperty("archive")]
			public bool Archive { get; set; }

			[JsonProperty("readOnly")]
			public bool ReadOnly { get; set; }

			[JsonProperty("unreadCount")]
			public int UnreadCount { get; set; }
		}

		class WaCreateGroupReturn
		{
			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("imageUrl")]
			public object ImageUrl { get; set; }

			[JsonProperty("createdAt")]
			public object CreatedAt { get; set; }

			[JsonProperty("inviteLink")]
			public object InviteLink { get; set; }

			[JsonProperty("metadata")]
			public Metadata Metadata { get; set; }
			[JsonProperty("status")]
			public int Status { get; set; }

			[JsonProperty("message")]
			public string Message { get; set; }

			[JsonProperty("errorCode")]
			public string ErrorCode { get; set; }

			[JsonProperty("errors")]
			public List<Error> Errors { get; set; }

		}
		class Error
		{
			[JsonProperty("errorCode")]
			public string ErrorCode { get; set; }

			[JsonProperty("message")]
			public string Message { get; set; }

			[JsonProperty("location")]
			public string Location { get; set; }
		}
		public class WaChat
		{
			public string id { get; set; }
			public string name { get; set; }
			public DateTime date { get; set; }
			public string type { get; set; }
			public string status { get; set; }
			public string waStatus { get; set; }
			public DateTime statusUpdatedAt { get; set; }
			public DateTime firstMessageAt { get; set; }
			public DateTime lastMessageAt { get; set; }
			public DateTime lastOutboundMessageAt { get; set; }
			public Stats stats { get; set; }
			public List<object> labels { get; set; }
			public WaContact contact { get; set; }
			public Group group { get; set; }
		}
		public class Group
		{
			public DateTime date { get; set; }
			public string owner { get; set; }
			public string imageUrl { get; set; }
			public int totalParticipants { get; set; }
			public List<Participant> participants { get; set; }
		}
	}
	public class WaDevice
	{
		public string id { get; set; }
		public string phone { get; set; }
		public string wid { get; set; }
		public string alias { get; set; }
		public string description { get; set; }
		public int version { get; set; }
		public string user { get; set; }
		public List<object> agents { get; }
		public string status { get; set; }
		public DateTime cancelAt { get; set; }
		public DateTime deletedAt { get; set; }
		public DateTime disabledAt { get; set; }
		public string lastMessage { get; set; }
		public DateTime lastMessageAt { get; }
		public List<object> logs { get; set; }
		public WaDeviceSession session { get; set; }
	}
	public class WaDeviceSession
	{
		public int uptime { get; set; }
		public string appVersion { get; set; }
		public string status { get; set; }
		public DateTime lastSyncAt { get; set; }
		public List<object> logs { get; }
	}

	public class WaContact
	{
		public string device { get; set; }
		public string wid { get; set; }
		public string groupId { get; set; }
		public string broadcastId { get; set; }
		public string phone { get; set; }
		public int countryCode { get; set; }
		public string kind { get; set; }
		public string name { get; set; }
		public string shortName { get; set; }
		public string displayName { get; set; }
		public string formattedName { get; set; }
		public string formattedShortName { get; set; }
		public DateTime syncedAt { get; set; }
		public Stats stats { get; set; }
		public Image image { get; set; }
		public Info info { get; set; }
	}
	public class Stats
	{
		public int messages { get; set; }
	}

	public class Image
	{
		public string url { get; set; }
	}

	public class Info
	{
		public string name { get; set; }
		public string surname { get; set; }
		public string title { get; set; }
		public string gender { get; set; }
		public string altPhone { get; set; }
		public string email { get; set; }
		public string emailAlt { get; set; }
		public string language { get; set; }
		public string company { get; set; }
		public string companyCode { get; set; }
		public string taxId { get; set; }
		public string currency { get; set; }
		public int timeOffset { get; set; }
		public string address { get; set; }
		public string city { get; set; }
		public string country { get; set; }
		public string note { get; set; }
	}
	public class WaGroupDetail
	{
		[JsonProperty("kind")]
		public string Kind { get; set; }

		[JsonProperty("isArchive")]
		public bool IsArchive { get; set; }

		[JsonProperty("isReadOnly")]
		public bool IsReadOnly { get; set; }

		[JsonProperty("totalParticipants")]
		public int TotalParticipants { get; set; }

		[JsonProperty("device")]
		public string Device { get; set; }

		[JsonProperty("wid")]
		public string Wid { get; set; }

		[JsonProperty("createdAt")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("isPinned")]
		public bool IsPinned { get; set; }

		[JsonProperty("lastMessageAt")]
		public DateTime LastMessageAt { get; set; }

		[JsonProperty("lastSyncAt")]
		public DateTime LastSyncAt { get; set; }

		[JsonProperty("muteExpiration")]
		public object MuteExpiration { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("participants")]
		public List<ParticipantV2> Participants { get; set; }

		[JsonProperty("unreadCount")]
		public int UnreadCount { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }
	}
	public class Participant
	{
		[JsonProperty("phone")]
		public string Phone { get; set; }

		[JsonProperty("admin")]
		public bool Admin { get; set; }
	}
	public class ParticipantV2
	{
		public string phone { get; set; }
		public bool isAdmin { get; set; }
		public bool isOwner { get; set; }
	}

	public class WaMedia2
	{
		public string file { get; set; }
		public string filename { get; set; }
		public string format { get; set; }
	}
	public class Retry
	{
		public int max { get; set; }
		public int delay { get; set; }
		public int count { get; set; }
		public DateTime lastRetryAt { get; set; }
	}

}
