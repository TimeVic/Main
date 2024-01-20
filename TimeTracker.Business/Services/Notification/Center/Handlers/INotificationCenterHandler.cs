using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Notification.Center.Handlers;

public interface INotificationCenterHandler<T>: IDomainService where T: IEntity
{
    Task Handle<T>(
        NotificationActionType action,
        UserEntity user,
        T entity
    );
}
