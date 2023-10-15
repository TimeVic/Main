using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Auth;

public interface IPasswordService: IDomainService
{
    UserEntity SetUserPassword(UserEntity user, string password);

    bool ValidatePassword(UserEntity user, string password);
}
