using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Orm.Constants;

namespace TimeTracker.Business.Services.Queue;

public interface IQueueService: IDomainService
{
    Task PushNotificationAsync(INotificationItemContext itemContext);

    Task PushDefaultAsync(IQueueItemContext itemContext);

    Task<int> ProcessAsync(QueueChannel channel, CancellationToken cancellationToken = default);
}
