using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Auth;

public interface IAuthorizationService: IDomainService
{
    Task<(string token, UserEntity user)> Login(string email, string password);
}
