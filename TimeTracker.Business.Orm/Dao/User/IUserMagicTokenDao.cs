using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserMagicTokenDao : IDomainService
{
    Task<UserMagicTokenEntity> GenerateNew(UserEntity user);

    Task<UserMagicTokenEntity?> GetAsync(Guid? id = null, string? token = null);
}
