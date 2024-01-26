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

            string error = null;
            try
            {
                if (channel == QueueChannel.Default)
                {
                    await ProcessDefaultItem(queueItem, cancellationToken);
                    processedCounter++;
                } 
                if (channel == QueueChannel.Notifications)
                {
                    await ProcessNotificationItem(queueItem, cancellationToken);
                    processedCounter++;
                } 
                else if (channel == QueueChannel.ExternalClient)
                {
                    await ProcessExternalClientItem(queueItem, cancellationToken);
                    processedCounter++;
                }
                else
                {
                    throw new Exception($"Channel handler is not exists: {channel}");
                }
            }
            catch (Exception e)
            {
                error = e.Message;
                _logger.LogError(e, e.Message);
            }
            await _queueDao.MarkAsProcessed(queueItem, error: error, cancellationToken: cancellationToken);
        }

        return processedCounter;
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
