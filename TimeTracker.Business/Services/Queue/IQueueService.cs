using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Orm.Constants;

namespace TimeTracker.Business.Services.Queue;

public interface IQueueService: IDomainService
{
    Task PushNotification(INotificationContext context);

    Task<int> Process(QueueChannel channel);
}
