using Microsoft.Extensions.Configuration;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserNotificationTokenDao: IUserNotificationTokenDao
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _expirationTimeout;

    public UserNotificationTokenDao(
        IDbSessionProvider sessionProvider,
        IConfiguration configuration
    )
    {
        _sessionProvider = sessionProvider;
        _configuration = configuration;
        _expirationTimeout = TimeSpan.FromDays(_configuration.GetValue<int>("App:Auth:AccessTokenLifetime"));
    }

    public async Task<UserNotificationTokenEntity> CreateNew(UserEntity user, string token)
    {
        var accessToken = new UserNotificationTokenEntity()
        {
            User = user,
            Token = token,
            CreateTime = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(accessToken);
        return accessToken;
    }
    
    public async Task<UserNotificationTokenEntity?> GetByToken(string accessToken)
    {
        return await _sessionProvider.CurrentSession.Query<UserNotificationTokenEntity>()
            .Where(item => item.Token == accessToken)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserNotificationTokenEntity?> GetByUser(UserEntity user)
    {
        return await _sessionProvider.CurrentSession.Query<UserNotificationTokenEntity>()
            .Where(item => item.User == user)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteByToken(string accessToken)
    {
        await _sessionProvider.CurrentSession.Query<UserNotificationTokenEntity>()
            .Where(item => item.Token == accessToken)
            .DeleteAsync();
    }
    
    public async Task<UserNotificationTokenEntity> Set(UserEntity user, string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new DataValidationException("Notification Token can not be empty");
        var tokenEntity = await GetByToken(token);
        if (tokenEntity == null)
        {
            return await CreateNew(user, token);
        }
        if (tokenEntity.User.Id != user.Id)
        {
            await DeleteByToken(token);
            return await CreateNew(user, token);
        }
        return tokenEntity;
    }
}
