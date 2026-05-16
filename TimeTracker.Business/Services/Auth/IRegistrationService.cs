using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Auth;

public interface IRegistrationService: IDomainService
{
    Task<UserEntity> CreatePendingUser(string email, string? languageCode = null);

    Task<UserEntity> ActivateUser(string verificationToken, string password);
}
