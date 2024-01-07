using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Services.Notification.Push;

public interface IGcmNotificationService: IDomainService
{
    Task SendTaskReminderNotification(TaskEntity task);
}
