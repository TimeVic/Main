using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class UserDao: IUserDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public UserDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<UserEntity?> GetExistsByUserName(string userName)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => item.UserName == userName.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetExistsByEmail(string email)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => item.Email == email.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity> CreatePendingUser(string email)
    {
        var user = new UserEntity
        {
            UserName = "",
            Email = email.Trim().ToLower(),
            VerificationToken = SecurityUtil.GetRandomString(32),
            PasswordHash = new byte[] {},
            PasswordSalt = new byte[] {},
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(user);
        return user;
    }
}
