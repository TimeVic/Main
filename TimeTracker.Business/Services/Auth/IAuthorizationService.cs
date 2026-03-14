using Domain.Abstractions;
using TimeTracker.Business.Dto.Auth;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Auth;

public interface IAuthorizationService: IDomainService
{
    Task<AuthResultDto> Login(string email, string password);
    
    Task<AuthResultDto> Login(UserEntity user);

    Task<AuthResultDto> GenerateNewJwtToken(string accessTokenString, string previousJwtToken);

    Task<AuthResultDto> GenerateNewJwtToken(UserAccessTokenEntity? accessToken);

    Task<UserEntity?> GetCurrentLoggedInUser();

    Guid? GetCurrentLoggedInUserUid();
}
