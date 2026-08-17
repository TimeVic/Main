using Domain.Abstractions;
using TimeTracker.Business.Clients.Smtp.Core;

namespace TimeTracker.Business.Notifications.Core;

public interface IEmailTemplateService : IDomainService
{
    Task<EmailBuilder> GetEmailBuilderAsync(string templateName, string recipientEmail);
}
