using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Travel.Core.Messaging.Email
{
    public interface IMailService
    {

        Task SendMailAsync(MailBase mail);
        Task SendMailAsync(MailBase mail, StringDictionary Replacements);
        void SendMail(MailBase mail);
        void SendMail(MailBase mail, StringDictionary Replacements);

        Task SendMailb(MailBase mail, string subject, string body);
    }

    public class SmtpEmailService : IMailService
    { 
        readonly SmtpConfig _smtpsettings;
        private readonly ILogger _logger;
        private readonly StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;


        public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<SmtpConfig> settingSvc)
        {
            _logger = logger;
            _smtpsettings = settingSvc.Value;
        }

        private SmtpClient GetSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = _smtpsettings.Server,
                Port = _smtpsettings.Port,
                EnableSsl = _smtpsettings.EnableSSl,
                UseDefaultCredentials = _smtpsettings.UseDefaultCredentials
            };

            if (!_smtpsettings.UseDefaultCredentials)
                client.Credentials = new NetworkCredential(_smtpsettings.UserName, _smtpsettings.Password);
            return client;
        }

        private async Task<MailMessage> BuildMailMessage(MailBase mail, StringDictionary replacements = null)
        {
            ValidateMail(mail);

            var sender = new MailAddress(mail.Sender, mail.SenderDisplayName);

            var mailMessage = new MailMessage()
            {
                Subject = mail.Subject,
                IsBodyHtml = mail.IsBodyHtml,
                From = sender,
            };

            var mailBody = !mail.BodyIsFile ? mail.Body : await GetEmailBodyTemplate(mail.BodyPath);

            if (replacements != null)
                mailBody = Replace(mailBody, replacements, false);

            mailMessage.Body = mailBody;

            if (mail.Attachments != null && mail.Attachments.Any())
                //for (int i = 0; i < mail.Attachments.Count-1; i++) {
                //    mailMessage.Attachments.Add(mail.Attachments.ElementAt(i));
                //}
                foreach (var attachment in mail.Attachments)
                    mailMessage.Attachments.Add(attachment);

            foreach (var to in mail.To)
                mailMessage.To.Add(to);

            if (mail.Bcc != null && mail.Bcc.Any())
                foreach (var bcc in mail.Bcc)
                    mailMessage.Bcc.Add(bcc);

            if (mail.CC != null && mail.CC.Any())
                foreach (var cc in mail.CC)
                    mailMessage.CC.Add(cc);

            return mailMessage;
        }

        void IMailService.SendMail(MailBase mail)
        {
            SendMail(mail, null);
        }

        void IMailService.SendMail(MailBase mail, StringDictionary replacements)
        {
            SendMail(mail, replacements);
        }

        async Task IMailService.SendMailAsync(MailBase mail)
        {
            await SendMailAsync(mail, null);
        }

        async Task IMailService.SendMailAsync(MailBase mail, StringDictionary replacements)
        {
            await SendMailAsync(mail, replacements);
        }

        async Task IMailService.SendMailb(MailBase mail, string subject, string bodl)
        {
            await SendMailAsync(mail, null);
        }


        protected virtual void SendMail(MailBase mail, StringDictionary replacements)
        {
            var message = BuildMailMessage(mail, replacements).Result;
            try {
                using (var _smtpClient = GetSmtpClient()) {
                    _smtpClient.Send(message);
                }
            }
            catch (Exception e) {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        protected virtual async Task SendMailAsync(MailBase mail, StringDictionary replacements)
        {
            var message = BuildMailMessage(mail, replacements).Result;
            try {
                using (var _smtpClient = GetSmtpClient()) {
                    await _smtpClient.SendMailAsync(message);
                }
            }
            catch (Exception e) {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        void ValidateMail(MailBase mail)
        {
            if (mail.To == null || !mail.To.Any())
                throw new ArgumentNullException("To");

            if (string.IsNullOrWhiteSpace(mail.Sender))
                throw new ArgumentNullException("Sender");

            if (string.IsNullOrWhiteSpace(mail.Subject))
                throw new ArgumentNullException("Subject");

            if (!mail.BodyIsFile && string.IsNullOrWhiteSpace(mail.Body))
                throw new ArgumentNullException("Body");

            if (mail.BodyIsFile && string.IsNullOrWhiteSpace(mail.BodyPath))
                throw new ArgumentNullException("BodyPath");
        }

        private Task<string> GetEmailBodyTemplate(string templateLocation)
        {
            return ReadTemplateFileContent(templateLocation);
        }

        private async Task<string> ReadTemplateFileContent(string templateLocation)
        {
            StreamReader sr;
            string body;
            try {
                if (templateLocation.ToLower().StartsWith("http")) {

                    var wc = new WebClient();
                    sr = new StreamReader(await wc.OpenReadTaskAsync(templateLocation));
                }

                else
                    sr = new StreamReader(templateLocation, Encoding.Default);

                body = sr.ReadToEnd();

                sr.Close();
            }
            catch (Exception e) {
                // _logger.Error(e.Message, e);
                throw e;
            }
            return body;
        }

      

        #region gotten from SmartStore/NopCommerce

        /// <summary>
        /// Replace all of the token key occurences inside the specified template text with corresponded token values
        /// </summary>
        /// <param name="template">The template with token keys inside</param>
        /// <param name="tokens">The sequence of tokens to use</param>
        /// <param name="htmlEncode">The value indicating whether tokens should be HTML encoded</param>
        /// <returns>Text with all token keys replaces by token value</returns>
        public string Replace(string template, StringDictionary tokens, bool htmlEncode)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException("template");

            if (tokens == null)
                throw new ArgumentNullException("tokens");

            foreach (string key in tokens.Keys) {
                string tokenValue = tokens[key];
                //do not encode URLs
                if (htmlEncode)
                    tokenValue = HtmlEncoder.Default.Encode(tokenValue);
                var replaceable = "{{" + key + "}}";
                template = Replace(template, replaceable, tokenValue);
            }
            return template;
        }

        private string Replace(string original, string pattern, string replacement)
        {
            if (_stringComparison == StringComparison.Ordinal) {
                return original.Replace(pattern, replacement);
            }
            else {
                int count, position0, position1;
                count = position0 = position1 = 0;
                int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
                char[] chars = new char[original.Length + Math.Max(0, inc)];
                while ((position1 = original.IndexOf(pattern, position0, _stringComparison)) != -1) {
                    for (int i = position0; i < position1; ++i)
                        chars[count++] = original[i];
                    for (int i = 0; i < replacement.Length; ++i)
                        chars[count++] = replacement[i];
                    position0 = position1 + pattern.Length;
                }
                if (position0 == 0) return original;
                for (int i = position0; i < original.Length; ++i)
                    chars[count++] = original[i];
                return new string(chars, 0, count);
            }
        }

       
        #endregion
    }
}