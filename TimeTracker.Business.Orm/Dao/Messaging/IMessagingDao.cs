using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public interface IMessagingDao: IScopedDomainService
{
    Task<MessagingConnectionEntity> SetConnection(UserEntity user, string connectionId);

    Task DeleteConnection(UserEntity user, string connectionId);
}
