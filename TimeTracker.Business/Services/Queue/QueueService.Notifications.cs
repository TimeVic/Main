using Autofac;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Notification.Abstractions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Notifications.Senders.Tasks.Comments;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.Queue;

public partial class QueueService: IQueueService
{
    private async Task ProcessNotificationItem(QueueEntity queueItem, CancellationToken cancellationToken = default)
    {
        var contextType = GetContextType(queueItem, typeof(BusinessNotificationsAssemblyMarker));
        if (contextType == null)
        {
            throw new Exception($"Notification context was not found in assembly: {queueItem.ContextType}");
        }
        if (IsContext<TestNotificationItemContext>(contextType))
        {
            await SendNotification<TestNotificationItemContext>(queueItem, cancellationToken);
        }
        else if (IsContext<RegistrationNotificationItemContext>(contextType))
        {
            await SendNotification<RegistrationNotificationItemContext>(queueItem, cancellationToken);
        }
        else if (IsContext<EmailVerifiedNotificationItemContext>(contextType))
        {
            await SendNotification<EmailVerifiedNotificationItemContext>(queueItem, cancellationToken);
        }
        else if (IsContext<TimeEntryAutoStoppedNotificationItemContext>(contextType))
        {
            await SendNotification<TimeEntryAutoStoppedNotificationItemContext>(queueItem, cancellationToken);
        }
        else if (IsContext<EmailVerificationNotificationItemContext>(contextType))
        {
            await SendNotification<EmailVerificationNotificationItemContext>(queueItem, cancellationToken);
        }
        else if (IsContext<TaskChangedNotificationContext>(contextType))
        {
            await SendNotification<TaskChangedNotificationContext>(queueItem, cancellationToken);
        }
        else if (IsContext<SetCommentNotificationContext>(contextType))
        {
            await SendNotification<SetCommentNotificationContext>(queueItem, cancellationToken);
        }
        else if (IsContext<ResetPasswordNotificationContext>(contextType))
        {
            await SendNotification<ResetPasswordNotificationContext>(queueItem, cancellationToken);
        }
        else
        {
            throw new Exception($"Incorrect notification context: {queueItem.ContextType}");
        }
    }
}
