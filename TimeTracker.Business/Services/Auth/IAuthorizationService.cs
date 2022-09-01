using Domain.Abstractions;

namespace TimeTracker.Business.Services.Auth;

public interface IAuthorizationService: IDomainService
{
    Task<string> Login(string email, string password);
}
