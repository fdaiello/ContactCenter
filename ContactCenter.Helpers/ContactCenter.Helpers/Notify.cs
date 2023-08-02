using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Clients.GsWhatsApp;
using ContactCenter.Infrastructure.Clients.Wassenger;
using ContactCenter.Infrastructure.Clients.MayTapi;
using ContactCenter.Infrastructure.Clients.Wpush;
using ContactCenter.Infrastructure.Clients.MpSms;
using ContactCenter.Infrastructure.Utilities;

namespace ContactCenter.Helpers
{
	public class Notify
	{
		private GsWhatsAppClient _gsWhatsAppClient;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _applicationDbContext;
		private WassengerClient _wassenger;
		private MayTapiClient _mayTapi;
		private MpSmsClient _mpSms;
		private readonly WpushClient _wpushClient;

		public Notify (IConfiguration configuration, ApplicationDbContext applicationDbContext, GsWhatsAppClient gsWhatsAppClient, WassengerClient wassenger, MayTapiClient mayTapi, WpushClient wpushClient, MpSmsClient mpSms)
		{
			_configuration = configuration;
			_applicationDbContext = applicationDbContext;
			_gsWhatsAppClient = gsWhatsAppClient;
			_wassenger = wassenger;
			_mayTapi = mayTapi;
			_wpushClient = wpushClient;
			_mpSms = mpSms;

		}
		// Envia uma notificação ( mensagem ) para um cliente
		public async Task<string> NotifyContact(string contactId, string message, string filename, string agentname)
		{

			string activityid = string.Empty;
			string whatsAppNumber = string.Empty;
			ChannelSubType chatChannelSubType = ChannelSubType.None;
			string customerPhone = string.Empty;

			bool hasBot = false;

			// Se o Agente enviou o comando Sair, muda a frase
			if (message != null && message.ToUpperInvariant() == "SAIR")
            {
				message = "Seu atendimento está sendo encerrado.";
            }

			// Acrescenta o nickname do agente no início da frase
			else if ( !string.IsNullOrEmpty(message) & !string.IsNullOrEmpty(agentname))
            {
				message = $"*- {agentname.Trim()}*\n{message}";
            }


			// Busca o Contato
			Contact contact = _applicationDbContext.Contacts
								.Where(s => s.Id == contactId)
								.FirstOrDefault();

			// Valida que encontrou
			if (contactId == null)
				return activityid;

			// Se o canal for Whats, Insta ou Email -> Vamos buscar o telefone, no Id, ou no campo mobilephone
			if (contact.ChannelType == ChannelType.WhatsApp | contact.ChannelType == ChannelType.Instagram | contact.ChannelType==ChannelType.SMS)
			{
				// Confere se tem o ID do ultimo canal usado, já salvo no registro do Customer
				if (!string.IsNullOrEmpty(contact.ChatChannelId))
				{
					// Busca o registro do canal
					ChatChannel chatChannel = _applicationDbContext.ChatChannels
												.Where(p => p.Id == contact.ChatChannelId)
												.Include(b => b.BotSettings)
												.FirstOrDefault();

					// Busca o numero do telefone do canal
					whatsAppNumber = chatChannel.PhoneNumber;

					// E o subtipo do canal
					chatChannelSubType = chatChannel.ChannelSubType;

					// Marca se tem Bot configurado
					hasBot = chatChannel.BotSettings?.Name != null;
				}

				// Se o contato não tem canal atribuido, devolve string vazia indicando erro de envio
				else
				{
					return "Erro: este contato não tem um canal definido.";
				}

				// Verifica se é um contato de grupo - tem 2 hifens
				if (contactId.Split("-").Length == 3)
				{
					// Se o Id tem 2 hifens, é o padrão de Grupo, e temos que retirar o prefixo do grupo, e concatenar o sufixo para formar o ID do grupo
					customerPhone = contactId.Split("-")[1] + "-" + contactId.Split("-")[2] + "@g.us";
				}
				// Contato de grupo - 18 digitos
				else if (contactId.Contains("-") && contactId.Split("-")[1].Length > 16)
				{
					// Outro padrão para Id de contato de grupo - 16 a 18 digitos
					customerPhone = contactId.Split("-")[1] + "@g.us";
				}
				// Verifica se o Id bate com Id original do WhatsApp
				else if (contactId.Split("-").Length == 2)
				{
					// o numero do whatsapp está dentro do ID, apos o hifen - o Id tem o prefixo do grupo
					// e se for Instagram, o ID é obtido da mesma forma
					customerPhone = contactId;
					if (customerPhone.Contains("-", StringComparison.InvariantCultureIgnoreCase))
						customerPhone = customerPhone.Split("-")[1];
				}
				// O contato foi criado por outro canal que não o Whats, e o seu Id não tem o seu numero do whats
				else
				{
					// Vamos ver se ele tem telefone
					if (!string.IsNullOrEmpty(contact.MobilePhone) && Utility.IsValidPhone(Utility.ClearStringNumber(contact.MobilePhone)))
					{
						// Busca o celuar salvo na lista do contato
						customerPhone = Utility.PadronizaCelular(Utility.ClearStringNumber(contact.MobilePhone));
						// Se não tem DDI
						if (customerPhone.Length == 11)
						{
							// Acrescenta o DDI do Brasil
							customerPhone = "55" + customerPhone;
						}
						// Se canal que estamos usando é do MayTapi
						if (chatChannelSubType == ChannelSubType.Alternate2)
						{
							// Chamamos a API do MayTapi para validar o numero
							CheckNumberRoot checkNumberRoot = await _mayTapi.CheckNumber(contact.ChatChannelId, customerPhone,contact.ChatChannel.Login, contact.ChatChannel.Password);
							if (checkNumberRoot.success && checkNumberRoot.result.canReceiveMessage)
							{
								customerPhone = checkNumberRoot.result.id.user;
							}
							else if ( checkNumberRoot.message != null )
							{
								// Retorna mensagem de erro que o canal não está ativo
								return $"Erro ao tentar validar o número de whats app.\n{checkNumberRoot.message}";
							}
							else
                            {
								// Retorna mensagem de erro
								return "Erro: este número não tem WhatsApp"; ;
							}
						}
					}
					// Não temos um telefone válido
					else
					{
						// Retorna string vazia, indicando que houve falha no envio - o padrão é devolver o Id da mensagem enviada e salva no banco
						return "Erro: este não é um número válido.";
					}
				}

			}

			if ( contact.ChannelType == ChannelType.None || contact.ChannelType == ChannelType.other)
            {
				return "Erro: o contato não tem um canal definido. Escolha o canal por onde a mensagem será enviada.";
            }
			// Confere se o canal do Contato é WhatsApp ou Instagram
			else if (contact.ChannelType == ChannelType.WhatsApp | contact.ChannelType == ChannelType.Instagram)
			{
				// Confere se tem arquivo
				if (!string.IsNullOrEmpty(filename))
				{

					// Monta a URL que aponta para o arquivo
					Uri fileUri = new Uri(_configuration.GetValue<string>("FileContainerUrl") + filename);

					// Remove caminhos do nome do arquivo ( nome do arquivo contem um caminho do folder do grupo )
					string[] afilename = filename.Split("/");
					filename = afilename[afilename.Length-1];

					// Confere qual API do WhatsApp está sendo usada
					if (chatChannelSubType == ChannelSubType.Oficial)
                    {
						// Envia direto via API GupShup
						GsWhatsAppClient.Mediatype mediaType;
						if (filename.EndsWith("ogg", StringComparison.InvariantCulture) || filename.EndsWith("wma", StringComparison.InvariantCulture) || filename.EndsWith("mp3", StringComparison.InvariantCulture) || filename.EndsWith("wav", StringComparison.InvariantCulture))
							mediaType = GsWhatsAppClient.Mediatype.audio;
						else if (filename.EndsWith("png", StringComparison.InvariantCulture) || filename.EndsWith("jpg", StringComparison.InvariantCulture) || filename.EndsWith("bmp", StringComparison.InvariantCulture) || filename.EndsWith("gif", StringComparison.InvariantCulture))
							mediaType = GsWhatsAppClient.Mediatype.image;
						else
							mediaType = GsWhatsAppClient.Mediatype.file;

						activityid = await _gsWhatsAppClient.SendMedia(whatsAppNumber, customerPhone, mediaType, filename, fileUri, null, contact.ChatChannel.Password).ConfigureAwait(false);

					}
                    else if ( chatChannelSubType == ChannelSubType.Alternate1)
                    {
						// Envia direto via API Wassenger
						WassengerClient.Mediatype mediaType;
						if (filename.EndsWith("ogg", StringComparison.InvariantCulture) || filename.EndsWith("wma", StringComparison.InvariantCulture) || filename.EndsWith("mp3", StringComparison.InvariantCulture) || filename.EndsWith("wav", StringComparison.InvariantCulture))
							mediaType = WassengerClient.Mediatype.audio;
						else if (filename.EndsWith("png", StringComparison.InvariantCulture) || filename.EndsWith("jpg", StringComparison.InvariantCulture) || filename.EndsWith("bmp", StringComparison.InvariantCulture) || filename.EndsWith("gif", StringComparison.InvariantCulture))
							mediaType = WassengerClient.Mediatype.image;
						else
							mediaType = WassengerClient.Mediatype.file;

						activityid = await _wassenger.SendMediaMessage(contact.ChatChannelId, customerPhone, mediaType, filename, fileUri).ConfigureAwait(false);

					}
					else if (chatChannelSubType == ChannelSubType.Alternate2 | contact.ChannelType == ChannelType.Instagram)
					{
						activityid = await _mayTapi.SendMediaMessage(contact.ChatChannelId, customerPhone, filename, fileUri, string.Empty, contact.ChatChannel.Login, contact.ChatChannel.Password).ConfigureAwait(false);
					}

				}

				// Confere se tem mensagem
				if ( !string.IsNullOrEmpty(message))
				{
					// Envia a mensagem direto via API do WhatsApp
					if (chatChannelSubType == ChannelSubType.Oficial)
						activityid = await _gsWhatsAppClient.SendText(whatsAppNumber, customerPhone, message, contact.ChatChannel.Password).ConfigureAwait(false);
					else if (chatChannelSubType == ChannelSubType.Alternate1)
						activityid = await _wassenger.SendMessage(contact.ChatChannelId, customerPhone, message).ConfigureAwait(false);
					else if (chatChannelSubType == ChannelSubType.Alternate2 | contact.ChannelType == ChannelType.Instagram)
						activityid = await _mayTapi.SendMessage(contact.ChatChannelId, customerPhone, message, contact.ChatChannel.Login, contact.ChatChannel.Password).ConfigureAwait(false);
				}


			}
			// Se o canal for SMS
			else if ( contact.ChannelType == ChannelType.SMS )
			{
				// Envia SMS pelo cliente de SMS da Mister Postman
				activityid = await _mpSms.SendSms("CC#Envios simples", customerPhone, message, contact.ChatChannel.Login, contact.ChatChannel.Password);
			}

			// Confere se é webchat mas ja passou 1 hora da ultima atividade
			else if ( contact.ChannelType == ChannelType.WebChat & Utility.HoraLocal().Subtract(contact.LastActivity).TotalHours > 1)
			{
				// Retorna activityid vazio indicando erro
				activityid = string.Empty;

			}
			else
			{
				// Busca a URL do Bot - salvo no grupo do cliente
				Group group = _applicationDbContext.Groups
							.Where(p => p.Id == contact.GroupId)
							.FirstOrDefault();

				if ( group != null)
                {
					// Marca que o contato está falando com o Agente - para o Bot não registrar novamente a mensagem no Banco de Dados
					contact.Status = ContactStatus.TalkingToAgent;
					_applicationDbContext.Contacts.Update(contact);
					await _applicationDbContext.SaveChangesAsync();

					// Envia a mensagem via notificação do BOT
					activityid = await NotifyBotCustomer(contactId, message, group.BotUrl).ConfigureAwait(false);
				}
			}

			// Devolve o ID da activity recebia apos o envio
			return activityid;
		}

		// Envia uma notificação para um customer via API do BOT
		public static async Task<string> NotifyBotCustomer(string customerid, string message, Uri BotUrl)
		{

			// Valida BotURL não nulo
			if (BotUrl == null)
				return string.Empty;

			// Monta a URL de Notificação
			string botnotifyUrl = BotUrl.ToString() + $"api/notify?key=Micky-2020*&id={customerid}&message={message}";

			try
			{
				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
				client.DefaultRequestHeaders.Add("User-Agent", "ContactCenter-2020");

				string httpreturn = await client.GetStringAsync(new Uri(botnotifyUrl)).ConfigureAwait(false);

				client.Dispose();

				if (httpreturn.Contains("Proactive message have been sent", StringComparison.Ordinal))
					// Gera um ID unico
					return Guid.NewGuid().ToString("N");
				else
					return string.Empty;

			}
			catch (HttpRequestException e)
			{
				Console.Write(e);
				return string.Empty;
			}

		}

		// Envia notificação para Agente via WebPush
		public async Task NotifyAgent(string applicationUserIdFrom, string applicationuserIdTo, string message)
        {
			ApplicationUser fromAgent = await _applicationDbContext.ApplicationUsers.FindAsync(applicationUserIdFrom).ConfigureAwait(false);
			ApplicationUser toAgent = await _applicationDbContext.ApplicationUsers.FindAsync(applicationuserIdTo).ConfigureAwait(false);

			if ( toAgent != null)
			await _wpushClient.SendNotification(
				$"Mensagem do: {fromAgent.NickName}",
				message,
				_configuration.GetValue<string>($"ContactCenterUrl"), toAgent.WebPushId).ConfigureAwait(false);
		}

		// Envia notificação para todos os agentes de um determinado Setor
		public async Task NotifyDepartment(string applicationUserIdFrom, int departmentId, string message)
		{
			ApplicationUser fromAgent = await _applicationDbContext.ApplicationUsers.FindAsync(applicationUserIdFrom).ConfigureAwait(false);

			var applicationUsers = await _applicationDbContext.ApplicationUsers
															.Where(p => p.DepartmentId == departmentId & !string.IsNullOrEmpty(p.WebPushId))
															.ToListAsync();

			foreach ( ApplicationUser applicationUser in applicationUsers)
            {
				await _wpushClient.SendNotification(
					$"Mensagem do: {fromAgent.NickName}",
					message,
					_configuration.GetValue<string>($"ContactCenterUrl"), applicationUser.WebPushId).ConfigureAwait(false);
			}

		}

	}
}

