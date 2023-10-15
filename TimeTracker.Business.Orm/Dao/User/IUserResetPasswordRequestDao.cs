using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserResetPasswordRequestDao: IDomainService
{
    Task<UserResetPasswordRequestEntity?> GetLast(UserEntity user);

    Task<UserResetPasswordRequestEntity> GenerateNew(UserEntity user);

    Task<UserResetPasswordRequestEntity?> GetByToken(string token);
}
