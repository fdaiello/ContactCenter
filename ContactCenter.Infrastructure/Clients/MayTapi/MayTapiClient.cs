using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static ContactCenter.Infrastructure.Clients.MayTapi.MayTapiClient;

namespace ContactCenter.Infrastructure.Clients.MayTapi
{
    // Classe para fazer as chamadas da API MayTapi para envio de mensagems para o WhatsApp
    public class MayTapiClient
    {

        // Variaveis de configuração para usar a API
        private readonly Uri apiUri;
        private readonly string defaultToken;
        private readonly string defaultProductId;

        // Injected logger and configuration
        private readonly ILogger _logger;
        private readonly DbContextOptionsBuilder<ApplicationDbContext> _dbOptionsBuilder;

        // Dictionary to save imported phones info
        private readonly IDictionary<string, int> importedPhones;
        private readonly IDictionary<string, int> totalPhones;
        private readonly IDictionary<string, bool> importingDevices;

        // Constructor
        public MayTapiClient(IOptions<MayTapiSettings> mayTapiSettings, ILogger<MayTapiClient> logger, DbContextOptionsBuilder<ApplicationDbContext> dbOptionsBuilder)
        {
            // Check if we got our needed settings
            if (mayTapiSettings == null)
                throw new ArgumentException("Argument missing:", nameof(mayTapiSettings));

            // Configure settings
            defaultToken = mayTapiSettings.Value.Token;
            defaultProductId = mayTapiSettings.Value.ProductId;
            apiUri = mayTapiSettings.Value.ApiUri;

            // Get injected objects
            _logger = logger;
            _dbOptionsBuilder = dbOptionsBuilder;

            // Initialize Dictionary
            importedPhones = new Dictionary<string, int>();
            totalPhones = new Dictionary<string, int>();
            importingDevices = new Dictionary<string, bool>();
        }
        public async Task<string> Redeploy(string phoneId, string productId, string token)
        {

            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            try
            {
                // Send text message that should be delivered now
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/redeploy");
                var request = new RestRequest(Method.GET);
                request.AddHeader("x-maytapi-key", token);

                // Executa o GET
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa a resposta
                RedeployResult redeployResult = JsonConvert.DeserializeObject<RedeployResult>(response.Content);

                // Devolve a mensagem
                if (redeployResult.message != null)
                {
                    return redeployResult.message;
                }
                else
                {
                    _logger.LogError("MayTapi.MayTapiClient.Redeploy: " + response);
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("MayTapi.MayTapiClient.Redeploy: " + ex.Message);
                return string.Empty;
            }

        }

        public async Task<string> SendMediaMessage(string phoneId, string destination, string filename, Uri contentUri, string text, string productId, string token)
        {

            try
            {
                // Confere se usa productId e token defaults
                if ( string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
				{
                    productId = defaultProductId;
                    token = defaultToken;
				}

                // Send text message that should be delivered now
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/SendMessage");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);

                // Message Body
                SendMediaMessageBody sendMediaMessageBody = new SendMediaMessageBody { message = contentUri.ToString(), to_number = destination, type = "media", text = text };
                string jsonPayload = JsonConvert.SerializeObject(sendMediaMessageBody);
                request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa o objeto mensagem
                SendMessageResponse sendMessageResponse = JsonConvert.DeserializeObject<SendMessageResponse>(response.Content);

                // Log Information
                _logger.LogInformation($"SendMessage. destination:{destination}, Uri: {contentUri}");

                // Devolve o Id da mensagem
                if (sendMessageResponse.data != null)
                    return sendMessageResponse.data.msgId;
				else
				{
                    _logger.LogError(response.Content);
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("MayTapi.MayTapiClient.SendMessage: " + ex.Message);
                return string.Empty;
            }

        }
        public async Task<string> SendMessage(string phoneId, string destination, string text, string productId, string token)
        {

            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            //Clean HTML from text
            text = Utility.CleanHTML(text);

            // Insert plus at internationa numbers, if there is not already
            if (!destination.StartsWith("+"))
                destination = "+" + destination;

            try
            {
                // Send text message that should be delivered now
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/SendMessage");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);

                // Message Body
                SendTextMessageBody sendMessageBody = new SendTextMessageBody { message = text, to_number = destination, type = "text" };
                string jsonPayload = JsonConvert.SerializeObject(sendMessageBody);
                request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa o objeto mensagem
                SendMessageResponse sendMessageResponse = JsonConvert.DeserializeObject<SendMessageResponse>(response.Content);

                // Devolve o Id da mensagem
                if (sendMessageResponse.data != null)
                {
                    // Log Information
                    _logger.LogInformation($"SendMessage. destination:{destination}, text: {text}");
                    return sendMessageResponse.data.msgId;
                }
                else
                {
                    _logger.LogError("MayTapi.MayTapiClient.SendMessage: " + response);
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("MayTapi.MayTapiClient.SendMessage: " + ex.Message);
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
                httpClient.DefaultRequestHeaders.Add("Apikey", defaultToken);
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                Stream contentStream = await (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false);

                request.Dispose();
                httpClient.Dispose();
                return (contentStream);

            }
            catch (Exception ex)
            {
                _logger.LogError("MayTapi.MayTapiClient.GetAudio: " + ex.Message);
                Stream stream = new MemoryStream();
                return (stream);
            }
        }
        public async Task<List<MtDevices>> ListPhones(string productId, string token)
        {

            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            try
            {
                // Get account devices
                var client = new RestClient($"{apiUri}/{productId}/listPhones");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa o retorno
                List<MtDevices> mtDevices = JsonConvert.DeserializeObject<List<MtDevices>>(response.Content);

                return (mtDevices);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException?.Message);
                return new List<MtDevices>();
            }

        }
        public async Task<byte[]> QrCode(string phoneId, string productId, string token)
        {
            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            try
            {
                // Get QrCode for a given PhoneId
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/qrCode");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                return (response.RawBytes);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException?.Message);
                return null;
            }

        }
        public async Task<MtStatus> PhoneStatus(string phoneId, string productId, string token)
        {

            // Check if has to use default values
            if ( string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(token))
			{
                productId = defaultProductId;
                token = defaultToken;
			}
            try
            {
                // Get Status for a given PhoneId
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/status");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa o retorno
                MtStatus mtPhone = JsonConvert.DeserializeObject<MtStatus>(response.Content);

                // Return mtPhone object
                return (mtPhone);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException?.Message);
                return new MtStatus();
            }

        }
        public async Task<CheckNumberRoot> CheckNumber(string phoneId, string phonenumber, string productId, string token)
        {

            phonenumber += "@c.us";

            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            try
            {
                // Get Status for a given PhoneId
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/checkNumberStatus?number={phonenumber}");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);
                request.Timeout = 10000;
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Se deu time-out
                if ( response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
				{
                    // retorna objeto com mensagem de erro
                    _logger.LogError("MayTapi CheckNumber: Gateway Time Out. PhoneId: {phoneId}");
                    return new CheckNumberRoot { result = new CheckNumberResult { id = null }, success = false, message = "Gateway time out" };
                }
                else if (string.IsNullOrEmpty(response.Content) )
                {
                    // retorna objeto com mensagem de erro
                    _logger.LogError("MayTapi CheckNumber: empty response");
                    return new CheckNumberRoot { result = new CheckNumberResult { id = null }, success = false, message = "empty response" };
                }
                // Confere qual foi o tipo de payload que voltou: volta diferente quando o numero tem whatsapp ou não
                else if (response.Content.Contains("_serialized"))
                {
                    // Desserializa o retorno - pay load de numero que tem whats
                    CheckNumberRoot checkNumberRoot = JsonConvert.DeserializeObject<CheckNumberRoot>(response.Content);

                    // Return Result
                    return checkNumberRoot;
                }
                else if ( response.Content.Contains("result"))
				{
                    /*  
                     *  Desserializa
                     *     A API foi modificada, mas dependendo do canal, podem voltar 2 tipos diferentes de payload
                     */
                    CheckNumberRoot checkNumberRoot;
                    if (response.Content.Contains("server"))
                    {
                        var checkNumberRootNoWhats = JsonConvert.DeserializeObject<CheckNumberRootNoWhats>(response.Content);
                        checkNumberRoot = new CheckNumberRoot { result = new CheckNumberResult { id = null, canReceiveMessage = checkNumberRootNoWhats.result.canReceiveMessage, isBusiness = checkNumberRootNoWhats.result.isBusiness, status = checkNumberRootNoWhats.result.status }, success = checkNumberRootNoWhats.success };
                    }
                    else
                    {
                        var checkNumberRootv2 = JsonConvert.DeserializeObject<CheckNumberRootv2>(response.Content);
                        checkNumberRoot = new CheckNumberRoot { result = new CheckNumberResult { id = null, canReceiveMessage = checkNumberRootv2.result.canReceiveMessage, isBusiness = checkNumberRootv2.result.isBusiness, status = checkNumberRootv2.result.status }, success = checkNumberRootv2.success };
                    }
                    return checkNumberRoot;
                }
                else
                {
                    // Desserializa - payload de erro
                    CheckNumberError checkNumberError = JsonConvert.DeserializeObject<CheckNumberError>(response.Content);
                    CheckNumberRoot checkNumberRoot = new CheckNumberRoot { result = new CheckNumberResult { id = null  }, success = false, message=checkNumberError.message };
                    return checkNumberRoot;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException?.Message);
                return new CheckNumberRoot { result = new CheckNumberResult { id = null }, success = false, message = ex.Message };
            }

        }
        /// <summary>
        /// Get lists of contacts of given deviceId
        /// </summary>
        /// <param name="deviceId">Id of device - attached to some WhatsAppNumber</param>
        /// <param name="size">max number of contacts to read in one call</param>
        /// <param name="page">page number - so that we can get more contacts in another call</param>
        /// <returns>List of Contacts</returns>
        public async Task<List<MtContact>> Contacts(string phoneId, string productId, string token)
        {

            // Confere se usa productId e token defaults
            if (string.IsNullOrEmpty(productId) | string.IsNullOrEmpty(token))
            {
                productId = defaultProductId;
                token = defaultToken;
            }

            try
            {

                // Contacts API
                var client = new RestClient($"{apiUri}/{productId}/{phoneId}/contacts");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-maytapi-key", token);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);

                // Desserializa o retorno
                ContactsResponse contactsResponse = JsonConvert.DeserializeObject<ContactsResponse>(response.Content);

                // Check if success
                if ( contactsResponse.success )
                    // Return contact list
                    return (contactsResponse.data);
                else
                    // Return empty list
                    return new List<MtContact>();

            }
            catch (Exception ex)
            {
                // registra erro
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException?.Message);

                // devolve lista vazia
                return new List<MtContact>();
            }

        }

        public async Task ImportContacts(string deviceId)
        {

            string contactId;

            if (!string.IsNullOrEmpty(deviceId))
            {
                // Check if deviceId is not present at Dictionary
                if (!importedPhones.ContainsKey(deviceId))
                {
                    // Insert device Id at dictionary
                    importedPhones.Add(deviceId, 0);
                    totalPhones.Add(deviceId, 0);
                    importingDevices.Add(deviceId, false);
                }
                // Check if is not already importing this device
                if (!importingDevices[deviceId])
                {
                    // Zera contadores - caso esteja importando novamente
                    importedPhones[deviceId] = 0;
                    totalPhones[deviceId] = 0;

                    try
                    {
                        // Crate a new instace of DbContext
                        ApplicationDbContext applicationDbContext = new ApplicationDbContext(_dbOptionsBuilder.Options);

                        // Get device info from Db
                        ChatChannel chatChannel = await applicationDbContext.ChatChannels.FindAsync(deviceId);
                        Contact contact;

                        if (chatChannel != null)
                        {
                            // Get contact list from device
                            List<MtContact> mtContacts = new List<MtContact>();
                            mtContacts = await Contacts(deviceId, chatChannel.Login, chatChannel.Password);

                            // Loop contacts
                            foreach (MtContact mtContact in mtContacts.Where(p => !string.IsNullOrEmpty(p.name) & p.type == "chat" & p.name != "You" & p.name != "WhatsApp"))
                            {

                                // Save phone number from Id
                                mtContact.phonenumber = mtContact.id.Split("@")[0].Replace("+", "");

                                // Confere se é um Id valido - as vezes vem Id zerado
                                if (mtContact.phonenumber.Length >= 11)
                                {
                                    // Gera o contact.Id
                                    contactId = chatChannel.GroupId + "-" + mtContact.phonenumber;

                                    // Para os numeros do Brasil, remove o 55 da frente
                                    if (mtContact.phonenumber.StartsWith("55") & mtContact.phonenumber.Length > 11)
                                        mtContact.phonenumber = mtContact.phonenumber.Substring(2);

                                    // If name is the phone number, clean it
                                    if (mtContact.name.StartsWith("+55"))
                                        mtContact.name = mtContact.name.Substring(3);
                                    else if (mtContact.name.StartsWith("+"))
                                        mtContact.name = mtContact.name.Substring(1);

                                    // Search contat at database
                                    contact = await applicationDbContext.Contacts.FindAsync(contactId);

                                    // If number is not alredy at database
                                    if (contact == null)
                                    {
                                        // Create new contact
                                        Contact newContact = new Contact { MobilePhone = mtContact.phonenumber, Name = mtContact.name ?? string.Empty, FullName = mtContact.name ?? string.Empty, Id = contactId, ApplicationUserId = chatChannel.ApplicationUserId, DepartmentId = chatChannel.DepartmentId, GroupId = chatChannel.GroupId, ChatChannelId = chatChannel.Id, ChannelType = chatChannel.ChannelType, LastText = string.Empty, FirstActivity = Utility.HoraLocal() };
                                        // Add to database
                                        await applicationDbContext.Contacts.AddAsync(newContact).ConfigureAwait(false);
                                        // Save changes
                                        await applicationDbContext.SaveChangesAsync().ConfigureAwait(false);

                                        // Increase counter
                                        importedPhones[deviceId]++;
                                    }
                                    totalPhones[deviceId]++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }
            }
        }
        public int ImportedContactsCount(string deviceId)
        {
            // Check if deviceId is not present at Dictionary

            if (importedPhones.ContainsKey(deviceId))
                // Return counter for device
                return importedPhones[deviceId];
            else
                // Return flag
                return -1;
        }
        public int ScannedContactsCount(string deviceId)
        {
            // Check if deviceId is not present at Dictionary

            if (totalPhones.ContainsKey(deviceId))
                // Return counter for device
                return totalPhones[deviceId];
            else
                // Return flag
                return -1;
        }

        public async Task<string> GetAndSaveStatus(string phoneId)
        {
            string status = "error";

            try
            {
                // Crate a new instace of DbContext
                ApplicationDbContext applicationDbContext = new ApplicationDbContext(_dbOptionsBuilder.Options);
                // Search channel at database
                ChatChannel channel = applicationDbContext.ChatChannels.Find(phoneId);

                // If we found the channel
                if (channel != null)
                {
                    // Call API to check its status
                    MtStatus mtStatus = await PhoneStatus(phoneId, channel.Login, channel.Password).ConfigureAwait(false);

                    // If we got a success response
                    if (mtStatus != null && mtStatus.success)
					{
                        // Adjust user friendly status
                        if (mtStatus.status.isQr)
                            status = "SCAN";
                        else
                            status = mtStatus.status.state == null ? "inicializando" : mtStatus.status.state.state;

                        // Save status to database
                        channel.Status = status;
                        channel.PhoneNumber = mtStatus.number;
                        await applicationDbContext.SaveChangesAsync();
                    }
                    // If we got some message
                    else if (mtStatus != null && mtStatus.message != null)
                    {
                        // return error message
                        status = mtStatus.message;
                    }
                }

                applicationDbContext.Dispose();
                
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }

            return status;
        }
        public class CheckNumberResultNoWhats
        {
            public MtContactId id { get; set; }
            public int status { get; set; }
            public bool isBusiness { get; set; }
            public bool canReceiveMessage { get; set; }
        }
        public class CheckNumberRootNoWhats
        {
            public bool success { get; set; }
            public CheckNumberResultNoWhats result { get; set; }
        }
        public class CheckNumberError
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string code { get; set; }
        }
        public class RedeployResult
        {
            public bool success { get; set; }
            public string message { get; set; }
        }

    }
    internal class SendTextMessageBody
	{
		public string to_number { get; set; }
		public string type { get; set; }
		public string message { get; set; }
	}
    internal class SendMediaMessageBody
    {
        public string to_number { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public string text { get; set; }

    }

    internal class SendMessageResponseData
	{
		public string chatId { get; set; }
		public string msgId { get; set; }
	}

    internal class SendMessageResponse
	{
		public bool success { get; set; }
		public SendMessageResponseData data { get; set; }
	}
    public class MtDevices
    {
        public int id { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public string type { get; set; }
    }
    public class MtContact
    {
        public string id { get; set; }
        public string phonenumber { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    internal class ContactsResponse
    {
        public bool success { get; set; }
        public List<MtContact> data { get; set; }
    }
    public class State
    {
        public string state { get; set; }
        public bool canSend { get; set; }
    }

    public class Status
    {
        public bool loggedIn { get; set; }
        public bool isQr { get; set; }
        public bool loading { get; set; }
        public bool connRetry { get; set; }
        public object message { get; set; }
        public State state { get; set; }
        public string version { get; set; }
        public object update { get; set; }
        public string number { get; set; }
    }

    public class MtStatus
    {
        public bool success { get; set; }
        public Status status { get; set; }
        public string number { get; set; }
        public string message { get; set; }
    }
    public class CheckNumberResult
    {
        public MtContactId id { get; set; }
        public int status { get; set; }
        public bool isBusiness { get; set; }
        public bool canReceiveMessage { get; set; }
    }
    public class CheckNumberResultv2
    {
        public string id { get; set; }
        public int status { get; set; }
        public bool isBusiness { get; set; }
        public bool canReceiveMessage { get; set; }
    }

    public class CheckNumberRoot
    {
        public bool success { get; set; }
        public string message { get; set; }
        public CheckNumberResult result { get; set; }
    }
    public class CheckNumberRootv2
    {
        public bool success { get; set; }
        public string message { get; set; }
        public CheckNumberResultv2 result { get; set; }
    }

    public class MtContactId
    {
        public string server { get; set; }
        public string user { get; set; }
        public string _serialized { get; set; }
    }
}
