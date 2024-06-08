using Microsoft.Extensions.Configuration;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserAccessTokenDao: IUserAccessTokenDao
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _expirationTimeout;

    public UserAccessTokenDao(
        IDbSessionProvider sessionProvider,
        IConfiguration configuration
    )
    {
        _sessionProvider = sessionProvider;
        _configuration = configuration;
        _expirationTimeout = TimeSpan.FromDays(_configuration.GetValue<int>("App:Auth:AccessTokenLifetime"));
    }

    public async Task<UserAccessTokenEntity> CreateNew(UserEntity user)
    {
        var accessToken = new UserAccessTokenEntity()
        {
            User = user,
            Token = SecurityUtil.GetRandomString(64),
            CreateTime = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow + _expirationTimeout
        };
        await _sessionProvider.CurrentSession.SaveAsync(accessToken);
        return accessToken;
    }
    
    public async Task<UserAccessTokenEntity?> GetByToken(string accessToken)
    {
        return await _sessionProvider.CurrentSession.Query<UserAccessTokenEntity>()
            .Where(item => item.Token == accessToken)
            .FirstOrDefaultAsync();
    }
    
    public async Task Delete(UserAccessTokenEntity accessToken)
    {
        await _sessionProvider.CurrentSession.Query<UserJwtTokenEntity>()
            .Where(item => item.AccessToken == accessToken)
            .DeleteAsync();
        await _sessionProvider.CurrentSession.Query<UserAccessTokenEntity>()
            .Where(item => item == accessToken)
            .DeleteAsync();
    }
}
