using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserAccessTokenDao: IDomainService
{
    Task<UserAccessTokenEntity> CreateNew(UserEntity user);

    Task<UserAccessTokenEntity?> GetByToken(string accessToken);

    Task<bool> HasJwtToken(UserAccessTokenEntity accessToken, string jwtToken);

    Task DeleteExpiredJwtTokens(UserAccessTokenEntity accessToken);

    Task Delete(UserAccessTokenEntity accessToken);
}
