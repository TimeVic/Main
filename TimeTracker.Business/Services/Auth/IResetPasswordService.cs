using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Auth;

public interface IResetPasswordService: IDomainService
{
    Task<UserResetPasswordRequestEntity?> Generate(UserEntity user);

    Task ChangePassword(string token, string password);
}
