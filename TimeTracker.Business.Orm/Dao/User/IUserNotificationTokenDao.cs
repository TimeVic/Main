using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserNotificationTokenDao: IDomainService
{
    Task<UserNotificationTokenEntity> Set(UserEntity user, string token);

    Task<UserNotificationTokenEntity?> GetByToken(string accessToken);

    Task<UserNotificationTokenEntity?> GetByUser(UserEntity user);
}
