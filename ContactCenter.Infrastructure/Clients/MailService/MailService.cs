using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace ContactCenter.Infrastructure.Clients.MailService
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;

        public MailService(ILogger<MailService> logger)
        {
            _logger = logger;
        }
        /* -------------------------------------------------------------------------------
         * Send Mail
         *
         * sender - Remetente { nome, email }
         * recipient - Destinatário { nome, email }
         * replyTo - Responder Para { nome, email }
         * subject - assunto
         * message - Html com a mensagem
         * listunsubscribe - URL da pagina para opt-out
         * smtpSettings - configurações do SMTP { host, port, login, pass, useSSL }
         * -------------------------------------------------------------------------------
         */
        public string SendMail(MailAddress sender, MailAddress recipient, MailAddress ReplyTo, string subject, string message, string listunsubscribe, SmtpSettings smtpSettings)
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
                mailMessage.Body = HtmlToPlainText(message);
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
                return msgId;
            }

            catch (Exception ex)
            {
                string msgerro = ex.Message;
                if (ex.InnerException != null)
                    msgerro += "\r\n" + ex.InnerException.Message;
                _logger.LogError(msgerro);
                return string.Empty;

            }
        }
        /*
         * Devolve um texto puro a partir de um HTML
         */
        public string HtmlToPlainText(string html)
        {
            // Remove script block


            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />

            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;

            // Remove style
            text = Regex.Replace(text, @"<style[\S\s]*style>", "");
            // Remove script
            text = Regex.Replace(text, @"<script[\S\s]*script>", "");
            // Remove office tags
            text = Regex.Replace(text, @"<o:[\S\s]*</o:.*>", "");

            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
        /* ---------------------------------------------------------------
         * Gera List Unsubscribe
         * ---------------------------------------------------------------
         */
        public string GeraListUnsubscribe(string email, string replyTo, string contactId, int sendingId)
        {
            // Ex:  List-Unsubscribe: <mailto:unsubscribe-mc.us9_5113e0e30f1135ea9067532e2.4dc806013d-42b1f25117@mailin1.us2.mcsv.net?subject=unsubscribe>, <http://triadps.us9.list-manage.com/unsubscribe?u=5113e0e30f1135ea9067532e2&id=ffe76e2de7&e=42b1f25117&c=4dc806013d>

            string LTpattern = $"<mailto:{replyTo}?subject=remover%20{email}>, <https://contact-center.azurewebsites.net/Unsubscribe?email={email}&contactId={contactId}&sendingId={sendingId}>";
            return LTpattern;
        }
    }
    /*
     * Configurações de SMTP
     */
    public class SmtpSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpLogin { get; set; }
        public string SmtpPass { get; set; }
        public bool EnableSsl { get; set; }
    }
}
