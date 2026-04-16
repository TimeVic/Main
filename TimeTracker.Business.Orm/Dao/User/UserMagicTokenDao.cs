using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserMagicTokenDao : IUserMagicTokenDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public UserMagicTokenDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<UserMagicTokenEntity> GenerateNew(UserEntity user)
    {
        var token = new UserMagicTokenEntity()
        {
            User = user,
            Token = SecurityUtil.GetBase58RandomString(256),
            ExpirationTime = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(token);
        return token;
    }

    public async Task<UserMagicTokenEntity?> GetByToken(string token)
    {
        return await _sessionProvider.CurrentSession.Query<UserMagicTokenEntity>()
            .Where(item => item.Token == token)
            .FirstOrDefaultAsync();
    }
}
