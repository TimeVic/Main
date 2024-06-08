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
    private async Task ProcessDefaultItem(QueueEntity queueItem, CancellationToken cancellationToken = default)
    {
        var contextType = GetContextType(queueItem, typeof(BusinessAssemblyMarker));
        if (contextType == null)
        {
            throw new Exception($"Queue context was not found in assembly: {queueItem.ContextType}");
        }
        if (IsContext<NotificationCenterPushRequestContext>(contextType))
        {
            await HandleQueueItem<NotificationCenterPushRequestContext>(queueItem, cancellationToken);
        }
        else
        {
            throw new Exception($"Incorrect queue context: {queueItem.ContextType}");
        }
    }
}
