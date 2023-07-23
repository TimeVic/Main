using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserResetPasswordRequestDao: IUserResetPasswordRequestDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public UserResetPasswordRequestDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }
    
    public async Task<UserResetPasswordRequestEntity?> GetLast(UserEntity user)
    {
        return await _sessionProvider.CurrentSession.Query<UserResetPasswordRequestEntity>()
            .Where(item => item.User == user)
            .OrderByDescending(item => item.CreateTime)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserResetPasswordRequestEntity?> GetByToken(string token)
    {
        return await _sessionProvider.CurrentSession.Query<UserResetPasswordRequestEntity>()
            .Where(item => item.VerificationToken == token)
            .OrderByDescending(item => item.CreateTime)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserResetPasswordRequestEntity> GenerateNew(UserEntity user)
    {
        var request = new UserResetPasswordRequestEntity()
        {
            User = user,
            VerificationToken = SecurityUtil.GetBase58RandomString(256),
            ExpirationTime = DateTime.UtcNow.AddMinutes(5),
            CreateTime = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(request);
        return request;
    }
}
