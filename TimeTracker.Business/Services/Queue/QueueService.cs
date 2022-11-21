using Autofac;
using Domain.Abstractions;
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
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.Queue;

public class QueueService: IQueueService
{
    private readonly IQueueDao _queueDao;
    private readonly ILogger<QueueService> _logger;
    private readonly IAsyncNotificationBuilder _notificationBuilder;
    private readonly ILifetimeScope _scope;

    public QueueService(
        IQueueDao queueDao,
        ILogger<QueueService> logger,
        IAsyncNotificationBuilder notificationBuilder,
        ILifetimeScope scope
    )
    {
        _queueDao = queueDao;
        _logger = logger;
        _notificationBuilder = notificationBuilder;
        _scope = scope;
    }

    public async Task PushDefaultAsync(IQueueItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.Default);
    }
    
    public async Task PushNotificationAsync(INotificationItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.Notifications);
    }
    
    public async Task PushExternalClientAsync(IExternalServiceItemContext itemContext)
    {
        await _queueDao.Push(itemContext, QueueChannel.ExternalClient);
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
            if (channel == QueueChannel.ExternalClient)
            {
                await ProcessExternalClientItem(queueItem, cancellationToken);
                processedCounter++;
            }
            else
            {
                _logger.LogError("Channel handler is not exists: {Channel}", channel);
            }
        }

        return processedCounter;
    }

    private async Task ProcessExternalClientItem(QueueEntity queueItem, CancellationToken cancellationToken = default)
    {
        var contextType = GetContextType(queueItem, typeof(BusinessAssemblyMarker));
        if (contextType == null)
        {
            _logger.LogError($"Queue context was not found in assembly: {queueItem.ContextType}");
            return;
        }
        if (IsContext<SendSetTimeEntryIntegrationRequestContext>(contextType))
        {
            await HandleQueueItem<SendSetTimeEntryIntegrationRequestContext>(queueItem, cancellationToken);
        }
        if (IsContext<SendDeleteTimeEntryIntegrationRequestContext>(contextType))
        {
            await HandleQueueItem<SendDeleteTimeEntryIntegrationRequestContext>(queueItem, cancellationToken);
        }
        else
        {
            _logger.LogError($"Incorrect queue context: {queueItem.ContextType}");
        }
    }
    
    private async Task ProcessNotificationItem(QueueEntity queueItem, CancellationToken cancellationToken = default)
    {
        var contextType = GetContextType(queueItem, typeof(BusinessNotificationsAssemblyMarker));
        if (contextType == null)
        {
            _logger.LogError($"Notification context was not found in assembly: {queueItem.ContextType}");
            return;
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

    private static bool IsContext<TConext>(Type contextType) where TConext: IQueueItemContext
    {
        return contextType == typeof(TConext);
    }
    
    private async Task SendNotification<TConext>(QueueEntity queueEntity, CancellationToken cancellationToken = default) where TConext: INotificationItemContext
    {
        var context = JsonHelper.DeserializeObject<TConext>(queueEntity.ContextData);
        if (context == null)
        {
            _logger.LogError("Notification context parsing error: {Type}", typeof(TConext));
            return;
        }

        await _notificationBuilder.SendAsync(context, cancellationToken);
    }
    
    private async Task HandleQueueItem<TConext>(QueueEntity queueEntity, CancellationToken cancellationToken = default) where TConext: IQueueItemContext
    {
        var context = JsonHelper.DeserializeObject<TConext>(queueEntity.ContextData);
        if (context == null)
        {
            _logger.LogError("Notification context parsing error: {Type}", typeof(TConext));
            return;
        }

        await _scope.Resolve<IAsyncQueueHandler<TConext>>().HandleAsync(context, cancellationToken);
    }
}
