using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Auth;

public interface IAuthorizationService: IDomainService
{
    Task<UserEntity> CreatePendingUser(string email);
}
