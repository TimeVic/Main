using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Notification.Push;

public interface IGcmNotificationService: IDomainService
{
    Task SendTaskReminderNotification(TaskEntity task);

    Task SendTaskCommentNotification(
        TaskCommentEntity comment,
        UserEntity receiverUser,
        UserEntity performedUsed,
        bool isChanged
    );

    Task SendTimeEntryRunningTooLongNotification(TimeEntryEntity timeEntry);
}
