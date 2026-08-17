using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserResetPasswordRequestDao: IUserResetPasswordRequestDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public UserResetPasswordRequestDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }
    
    public async Task<UserResetPasswordRequestEntity?> GetAsync(
        Guid? id = null,
        string? verificationToken = null,
        UserEntity? user = null
    )
    {
        if (id == null && string.IsNullOrWhiteSpace(verificationToken) && user == null)
            throw new ArgumentException("Either id, verification token, or user must be provided.");

        IQueryable<UserResetPasswordRequestEntity> query = _sessionProvider.CurrentSession.Query<UserResetPasswordRequestEntity>()
            .Fetch(item => item.User)
            .ThenFetch(user => user.Language);
        if (id.HasValue)
            query = query.Where(item => item.Id == id.Value);
        if (!string.IsNullOrWhiteSpace(verificationToken))
            query = query.Where(item => item.VerificationToken == verificationToken);
        if (user != null)
            query = query.Where(item => item.User == user);

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserResetPasswordRequestEntity> GenerateNew(UserEntity user)
    {
        var request = new UserResetPasswordRequestEntity()
        {
            User = user,
            VerificationToken = SecurityUtil.GetBase58RandomString(256),
            ExpirationTime = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(request);
        return request;
    }
}
