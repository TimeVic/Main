using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Logging.Client.GrayLog;
using TimeTracker.Business.Logging.Dto;

namespace TimeTracker.Business.Clients.Smtp
{
    public class SmtpClientService : ISmtpClientService
    {
        private readonly SmtpSettings _smtpSettings;

        private readonly MailAddress _defaultFromAddress;
        private readonly NetworkCredential _credentials;
        private readonly ILogger<SmtpClientService> _logger;
        private readonly IGraylogClient _graylogClient;

        private char[] Separators = ";".ToCharArray(); // for splitting lists of emails

        public SmtpClientService(
            IConfiguration configuration, 
            ILogger<SmtpClientService> logger,
            IGraylogClient graylogClient
        )
        {
            _smtpSettings = new SmtpSettings(configuration);
            _defaultFromAddress = new MailAddress(_smtpSettings.EmailFrom ?? string.Empty, _smtpSettings.UserNameFrom);
            _credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
            _logger = logger;
            _graylogClient = graylogClient;
        }

        private void ParseEmails(MailAddressCollection collection, string? emails)
        {
            if (string.IsNullOrWhiteSpace(emails))
                return;

            var emailArray = emails.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var em in emailArray)
            {
                collection.Add(em);
            }
        }

        // returns "" on success, or text of SMTP exceptions etc
        public string SendEmail(
            string to,
            EmailBuilder emailBuilder,
            string? bcc
        )
        {
            try
            {
                emailBuilder.Build();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                LogEmail(_defaultFromAddress.Address, to, emailBuilder.Subject, emailBuilder.Body, null, bcc, e.Message);
                return e.Message;
            }
            return SendEmail(null, to, emailBuilder.Subject, emailBuilder.Body, null, bcc);
        }

        // returns "" on success, or text of SMTP exceptions etc
        public string SendEmail(
            string to,
            string subject,
            string body,
            string? bcc
        )
        {
            return SendEmail(null, to, subject, body, null, bcc);
        }

        // returns "" on success, or text of SMTP exceptions etc
        public string SendEmail(
            string? from,
            string to,
            string subject,
            string body,
            string? cc,
            string? bcc
        )
        {
            _logger.LogDebug($"Send email to {to} from {from} with subject {subject.MySubstring(0, 15)}");
            
            string res = string.Empty;
            using var message = new MailMessage();

            message.From = string.IsNullOrWhiteSpace(from) ? _defaultFromAddress : new MailAddress(from);
            ParseEmails(message.ReplyToList, message.From?.Address);

            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("'to' parameter is required");

            ParseEmails(message.To, to);
            ParseEmails(message.CC, cc);
            ParseEmails(message.Bcc, bcc);

            message.Subject = subject.RemoveNewLineSymbols();
            message.IsBodyHtml = true;
            message.Body = body;

            message.Priority = MailPriority.Normal;

            res = SendViaSmtpClient(message);
            LogEmail(message.From?.Address ?? _defaultFromAddress.Address, to, subject, body, cc, bcc, res);
            return res;

        }

        private string SendViaSmtpClient(MailMessage message)
        {
            using var smtpClient = new System.Net.Mail.SmtpClient(_smtpSettings.Server);
            smtpClient.Port = _smtpSettings.Port;
            smtpClient.Credentials = _credentials;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;

            try
            {
                smtpClient.Send(message);
                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Email sending failed for {EmailTo}", message.To);
                return e.Message;
            }
        }

        private void LogEmail(
            string from,
            string to,
            string subject,
            string body,
            string? cc,
            string? bcc,
            string? error
        )
        {
            _graylogClient.LogEmail(new EmailLogDto
            {
                EmailFrom = from,
                EmailTo = to,
                EmailSubject = subject,
                EmailBody = body,
                EmailCc = cc,
                EmailBcc = bcc,
                EmailSendingError = string.IsNullOrWhiteSpace(error) ? null : error
            });
        }
    }
}
