using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserResetPasswordRequestDao: IDomainService
{
    Task<UserResetPasswordRequestEntity> GenerateNew(UserEntity user);

    Task<UserResetPasswordRequestEntity?> GetAsync(
        Guid? id = null,
        string? verificationToken = null,
        UserEntity? user = null
    );
}
