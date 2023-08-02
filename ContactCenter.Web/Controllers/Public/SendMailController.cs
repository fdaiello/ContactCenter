using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Clients.MailService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace ContactCenter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [AllowAnonymous]
    public class SendMailController : ControllerBase
	{

        [HttpPost]
		public ActionResult<SendMailResult> SendMail(Email email)
		{
			SmtpSettings smtpSettings = new SmtpSettings()
			{
				EnableSsl = email.SmtpSettings.EnableSsl,
				SmtpHost = email.SmtpSettings.SmtpHost,
				SmtpLogin=email.SmtpSettings.SmtpLogin,
				SmtpPass=email.SmtpSettings.SmtpPass,
				SmtpPort=email.SmtpSettings.SmtpPort

			};

			MailAddress from = new MailAddress(email.From,email.FromName);
			MailAddress to = new MailAddress(email.To, email.ToName);
			MailAddress replyTo = new MailAddress(email.ReplyTo);

			string msg = SendMail(from, to, replyTo, email.Subject, email.Text, null, smtpSettings);

			SendMailResult sendMailResult = new SendMailResult() { Message = msg };
			return sendMailResult;

        }

        private string SendMail(MailAddress sender, MailAddress recipient, MailAddress ReplyTo, string subject, string message, string listunsubscribe, SmtpSettings smtpSettings)
        {

            // create new smtp client
            SmtpClient smtpClient = new SmtpClient("127.0.0.1");

            // create new message Id
            string msgId = System.Guid.NewGuid().ToString();

            try
            {
                // create new mail message
                MailMessage mailMessage = new MailMessage();

                // set the addresses
                mailMessage.From = sender;
                mailMessage.To.Add(recipient);

                // set the content
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = false;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;

                // Adiciona versao texto
                var contenttype = new System.Net.Mime.ContentType("text/html; charset=iso-8859-1");
                mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message, contenttype));

                // reply to
                if (ReplyTo != null)
                    mailMessage.ReplyToList.Add(ReplyTo);

                // Adiciona MessageID
                mailMessage.Headers.Add("Message-ID", msgId);
                mailMessage.Headers.Add("x-smtplw", msgId);

                // List Unsubscribe
                if (!string.IsNullOrEmpty(listunsubscribe))
                    mailMessage.Headers.Add("List-Unsubscribe", listunsubscribe);

                // configure smtp client
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Host = smtpSettings.SmtpHost;
                smtpClient.Port = smtpSettings.SmtpPort;
                smtpClient.EnableSsl = smtpSettings.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(smtpSettings.SmtpLogin, smtpSettings.SmtpPass);

                // send the message
                smtpClient.Send(mailMessage);

                // return
                return "ok";
            }

            catch (Exception ex)
            {
                string msgerro = ex.Message;
                if (ex.InnerException != null)
                    msgerro += "\r\n" + ex.InnerException.Message;
                return msgerro;
            }
        }


    }
    public class Email
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public string ReplyTo { get; set; }
        public string Text { get; set; }
        public SmtpSettings SmtpSettings { get; set; }
    }
    public class SendMailResult
    {
        public string Message { get; set; }
    }

}
