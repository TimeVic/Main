using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserSocialLoginDao: BaseDao, IUserSocialLoginDao
{
    public UserSocialLoginDao(ILifetimeScope scope): base(scope)
    {
    }

    public async Task<UserSocialLoginEntity?> GetByUserAsync(UserEntity user)
    {
        return await Session.Query<UserSocialLoginEntity>()
            .Where(item => item.User.Id == user.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<UserSocialLoginEntity?> GetByProviderIdsAsync(string? googleId, string? facebookId, string? appleId)
    {
        var hasGoogleId = !string.IsNullOrWhiteSpace(googleId);
        var hasFacebookId = !string.IsNullOrWhiteSpace(facebookId);
        var hasAppleId = !string.IsNullOrWhiteSpace(appleId);

        if (
            !hasGoogleId
            && !hasFacebookId
            && !hasAppleId
        )
        {
            return null;
        }

        return await Session.Query<UserSocialLoginEntity>()
            .Fetch(item => item.User)
            .Where(item =>
                (hasGoogleId && item.GoogleId == googleId)
                || (hasFacebookId && item.FacebookId == facebookId)
                || (hasAppleId && item.AppleId == appleId)
            )
            .FirstOrDefaultAsync();
    }

    public async Task<UserSocialLoginEntity> SaveAsync(UserSocialLoginEntity socialLogin)
    {
        await Session.SaveOrUpdateAsync(socialLogin);
        return socialLogin;
    }
}
