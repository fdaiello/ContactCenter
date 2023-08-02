using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Utilities;

/*
 * Bounce controller
 * Usado como WebHook para marcar erros de envio de emails
 */

namespace ContactCenter.Web.Controllers.Webhook
{
	[Route("api/[controller]")]
	[ApiController]
	public class BounceController : ControllerBase
	{
		// Database context
		private readonly ApplicationDbContext _context;

		// Constructor
		public BounceController(ApplicationDbContext context) 
		{
			_context = context;
		}

		// POST api/bounce/smtplw
		[HttpPost("smtplw")]
		public async Task<ActionResult> Post([FromForm] string value)
		{
			string bounce_description = Request.Form["bounce_description"];
			string bounce_code = Request.Form["bounce_code"];
			string sender = Request.Form["sender"];
			string recipient = Request.Form["to"];
			string subject = Request.Form["subject"];
			string activityId = Request.Form["x-smtplw"];

			if (!string.IsNullOrEmpty(activityId))
				await MarkBouncedMsgById(activityId, bounce_code, bounce_description, recipient);

			return Ok();
		}
		// Marca que uma mensagem recebeu um codigo de bounce - com base no id da mensagem
		private async Task MarkBouncedMsgById(string activityId, string bounce_code, string bounce_descriptor, string recipient)
		{
			// Localiza a mensagem - com base no activity Id que foi passado no header
			ChattingLog chattingLog = await _context.ChattingLogs
									.Where(p => p.ActivityId == activityId)
									.FirstOrDefaultAsync();

			// Se encontrou
			if (chattingLog != null)
			{
				// Marca que deu erro de entrega
				chattingLog.Status = MsgStatus.Failed;
				// Se deu erro permanente
				if (bounce_code.StartsWith("5"))
				{
					chattingLog.FailedReason = recipient + " inválido";
				}
				else
				{
					chattingLog.FailedReason = bounce_descriptor;
				}
				chattingLog.StatusTime = Utility.HoraLocal();
				_context.ChattingLogs.Update(chattingLog);
			}

			// Se o codigo do bounce indica erro permanente: 5.xxx
			if (bounce_code.StartsWith("5"))
			{
				// Localiza os contatos pelo email
				IEnumerable<Contact> contacts = await _context.Contacts
												.Where(p => p.Email == recipient)
												.ToListAsync();
				// Se achou algum 
				if ( contacts.Any())
				{
					// Varre a lista dos contatos encontrados com o email que deu erro
					foreach ( Contact contact in contacts)
					{
						// Marca que o email deu erro - como não temos campo para isto, vamos concatenar 'inválido'
						contact.Email += "' inválido";
						_context.Contacts.Update(contact);
					}
				}
			}

			// Salva no banco
			await _context.SaveChangesAsync();
		}
	}
}
