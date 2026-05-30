using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserSocialLoginDao: IDomainService
{
    Task<UserSocialLoginEntity?> GetByUserAsync(UserEntity user);

    Task<UserSocialLoginEntity?> GetByProviderIdsAsync(string? googleId, string? facebookId, string? appleId);

    Task<UserSocialLoginEntity> SaveAsync(UserSocialLoginEntity socialLogin);
}
