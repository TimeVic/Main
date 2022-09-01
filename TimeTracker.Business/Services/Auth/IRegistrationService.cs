using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Auth;

public interface IRegistrationService: IDomainService
{
    Task<UserEntity> CreatePendingUser(string email);

    Task<UserEntity> ActivateUser(string verificationToken, string password);
}
