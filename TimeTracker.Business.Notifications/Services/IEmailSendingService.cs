using Domain.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;

namespace TimeTracker.Business.Notifications.Services
{
    public interface IEmailSendingService : IDomainService
    {
        public string SendEmail(
            string to,
            EmailBuilder emailBuilder,
            string bcc
        );

        public string SendEmail(
            string to,
            string subject,
            string body,
            string bcc
        );

        public string SendEmail(
            string from,
            string to,
            string subject,
            string body,
            string cc,
            string bcc
        );
    }
}
