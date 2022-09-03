using Microsoft.Extensions.Logging;
using Notification.Abstractions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Notifications.Senders;
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

    public async Task PushNotification(INotificationContext context)
    {
        await _queueDao.Push(context, QueueChannel.Notifications);
    }
    
    public async Task<int> Process(QueueChannel channel)
    {
        var processedCounter = 0;
        while (true)
        {
            var queueItem = await _queueDao.GetTop(channel);
            if (queueItem == null)
            {
                break;
            }

            if (channel == QueueChannel.Notifications)
            {
                await ProcessNotificationItem(queueItem);
                processedCounter++;
            }
            else
            {
                _logger.LogError("Channel handler is not exists: {Channel}", channel);
            }
        }

        return processedCounter;
    }

    private async Task ProcessNotificationItem(QueueEntity queueItem)
    {
        var errorMessage = $"Incorrect notification context: {queueItem.ContextType}";
        var contextType = GetContextType(queueItem, typeof(BusinessNotificationsAssemblyMarker));
        if (contextType == null)
        {
            _logger.LogError(errorMessage);
        }
        if (IsContext<TestNotificationContext>(contextType))
        {
            await SendNotification<TestNotificationContext>(queueItem);
        }
        if (IsContext<RegistrationNotificationContext>(contextType))
        {
            await SendNotification<RegistrationNotificationContext>(queueItem);
        }
        if (IsContext<EmailVerifiedNotificationContext>(contextType))
        {
            await SendNotification<EmailVerifiedNotificationContext>(queueItem);
        }
        else
        {
            _logger.LogError(errorMessage);
        }
    }
    
    private Type GetContextType(QueueEntity queueItem, Type markerType)
    {
        var activationResult = Activator.CreateInstance(
            markerType.Assembly.GetName().Name,
            queueItem.ContextType
        );
        return activationResult.Unwrap().GetType();
    }

    private bool IsContext<TConext>(Type contextType) where TConext: INotificationContext
    {
        return contextType == typeof(TConext);
    }
    
    private async Task SendNotification<TConext>(QueueEntity queueEntity) where TConext: INotificationContext
    {
        var context = JsonHelper.DeserializeObject<TConext>(queueEntity.ContextData);
        if (context == null)
        {
            _logger.LogError("Notification context parsing error: {Type}", typeof(TConext));
            return;
        }

        await _notificationBuilder.SendAsync(context);
    }
}
