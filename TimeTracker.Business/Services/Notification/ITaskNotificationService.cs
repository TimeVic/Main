using Domain.Abstractions;

namespace TimeTracker.Business.Services.Notification;

public interface ITaskNotificationService: IDomainService
{
    Task<bool> NotifyAboutTaskChanges();

    Task<bool> SendReminderNotification();
}
