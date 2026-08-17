using Domain.Abstractions;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Notifications.Core;

public interface IEmailTemplateService : IDomainService
{
    EmailBuilder GetEmailBuilder(string templateName, UserEntity recipient);

    EmailBuilder GetEmailBuilder(string templateName, string languageCode);
}
