using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Utilities;

/*
 * OpenMsgController
 * Usado como WebHook para marcar mensagens de email lidas
 */
namespace ContactCenter.Web.Controllers.Webhook
{
	[Route("api/[controller]")]
	[ApiController]
	public class OpenMsgController : ControllerBase
	{
		// Database context
		private readonly ApplicationDbContext _context;

		// Constructor
		public OpenMsgController(ApplicationDbContext context) 
		{
			_context = context;
		}

		// POST api/openmsg/smtplw
		[HttpPost("smtplw")]
		public async Task<ActionResult> Post([FromForm] string value)
		{
			string opened_at = Request.Form["opened-at"];
			string sender = Request.Form["sender"];
			string to = Request.Form["to"];
			string subject = Request.Form["subject"];
			string activityId = Request.Form["x-smtplw"];

			// Se recebeu o ActivityId enviado no header x-smtplw
			if ( !string.IsNullOrEmpty(activityId))
			{
				// Marca que a mensagem foi lida
				await MarkMsgOpenById(activityId);
			}

			return Ok();
		}

		// Marca que uma mensagem foi aberta - com base no id da mensagem
		private async Task MarkMsgOpenById(string activityId)
		{
			// Localiza a mensagem - com base no activity Id que foi passado no header
			ChattingLog chattingLog = await _context.ChattingLogs
									.Where(p => p.ActivityId == activityId)
									.FirstOrDefaultAsync();

			// Se encontrou
			if (chattingLog != null)
			{
				// Marca que foi lida
				chattingLog.Status = MsgStatus.Read;
				chattingLog.StatusTime = Utility.HoraLocal();
				// E salva
				_context.ChattingLogs.Update(chattingLog);
				await _context.SaveChangesAsync();
			}
		}
	}
}
