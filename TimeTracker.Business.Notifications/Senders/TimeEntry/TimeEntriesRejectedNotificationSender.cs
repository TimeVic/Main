using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry;

public class TimeEntriesRejectedNotificationSender : IAsyncQueueHandler<TimeEntriesRejectedNotificationItemContext>
{
    private readonly ISmtpClientService _smtpClientService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceDao _workspaceDao;

    public TimeEntriesRejectedNotificationSender(
        ISmtpClientService smtpClientService,
        IEmailTemplateService emailTemplateService,
        IUserDao userDao,
        IWorkspaceDao workspaceDao
    )
    {
        _smtpClientService = smtpClientService;
        _emailTemplateService = emailTemplateService;
        _userDao = userDao;
        _workspaceDao = workspaceDao;
    }

    public async Task HandleAsync(
        TimeEntriesRejectedNotificationItemContext context,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userDao.GetById(context.UserId);
        if (user == null)
            return;

        var workspace = await _workspaceDao.GetById(context.WorkspaceId);

        var emailBuilder = _emailTemplateService.GetEmailBuilder("TimeEntriesRejectedNotification.htm", user);
        emailBuilder.AddPlaceholder("rejectionReason", context.RejectionReason);
        emailBuilder.AddPlaceholder("entriesCount", context.TimeEntryIds.Count.ToString());
        emailBuilder.AddPlaceholder("workspaceName", workspace?.Name ?? string.Empty);
        emailBuilder.AddPlaceholder("timeEntriesUrl", workspace != null ? $"/board/{workspace.Id}" : "/");

        _smtpClientService.SendEmail(user.Email, emailBuilder, null);
    }
}
