using Microsoft.Extensions.Logging;
using Notification.Abstractions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Queue;

public class QueueService: IQueueService
{
    private readonly IQueueDao _queueDao;
    private readonly ILogger<QueueService> _logger;
    private readonly IAsyncNotificationBuilder _notificationBuilder;

    public QueueService(
        IQueueDao queueDao,
        ILogger<QueueService> logger,
        IAsyncNotificationBuilder notificationBuilder
    )
    {
        _queueDao = queueDao;
        _logger = logger;
        _notificationBuilder = notificationBuilder;
    }

    public async Task PushNotificationAsync(INotificationContext context)
    {
        await _queueDao.Push(context, QueueChannel.Notifications);
    }
    
    public async Task<int> ProcessAsync(QueueChannel channel, CancellationToken cancellationToken = default)
    {
        var processedCounter = 0;
        while (true)
        {
            var queueItem = await _queueDao.GetTop(channel, cancellationToken);
            if (queueItem == null)
            {
                break;
            }

            if (channel == QueueChannel.Notifications)
            {
                await ProcessNotificationItem(queueItem, cancellationToken);
                processedCounter++;
            }
            else
            {
                _logger.LogError("Channel handler is not exists: {Channel}", channel);
            }
        }

        return processedCounter;
    }

    private async Task ProcessNotificationItem(QueueEntity queueItem, CancellationToken cancellationToken = default)
    {
        var contextType = GetContextType(queueItem, typeof(BusinessNotificationsAssemblyMarker));
        if (contextType == null)
        {
            _logger.LogError($"Notification context was not found in assembly: {queueItem.ContextType}");
            return;
        }
        if (IsContext<TestNotificationContext>(contextType))
        {
            await SendNotification<TestNotificationContext>(queueItem, cancellationToken);
        }
        else if (IsContext<RegistrationNotificationContext>(contextType))
        {
            await SendNotification<RegistrationNotificationContext>(queueItem, cancellationToken);
        }
        else if (IsContext<EmailVerifiedNotificationContext>(contextType))
        {
            await SendNotification<EmailVerifiedNotificationContext>(queueItem, cancellationToken);
        }
        else if (IsContext<TimeEntryAutoStoppedNotificationContext>(contextType))
        {
            await SendNotification<TimeEntryAutoStoppedNotificationContext>(queueItem, cancellationToken);
        }
        else
        {
            _logger.LogError($"Incorrect notification context: {queueItem.ContextType}");
        }
    }
    
    private static Type? GetContextType(QueueEntity queueItem, Type markerType)
    {
        var activationResult = Activator.CreateInstance(
            markerType.Assembly.GetName().Name,
            queueItem.ContextType
        );
        return activationResult?.Unwrap()?.GetType();
    }

    private static bool IsContext<TConext>(Type contextType) where TConext: INotificationContext
    {
        return contextType == typeof(TConext);
    }
    
    private async Task SendNotification<TConext>(QueueEntity queueEntity, CancellationToken cancellationToken = default) where TConext: INotificationContext
    {
        var context = JsonHelper.DeserializeObject<TConext>(queueEntity.ContextData);
        if (context == null)
        {
            _logger.LogError("Notification context parsing error: {Type}", typeof(TConext));
            return;
        }

        await _notificationBuilder.SendAsync(context, cancellationToken);
    }
}
