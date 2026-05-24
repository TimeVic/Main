using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Client.Core.Services
{
    public interface IAuthorizationService
    {
        Task<bool> LoginAsync(LoginRequest model);
        void Login(string accessToken, string jwtToken, UserDto user);
        void Login(UserDto user);
        Task LogoutAsync();
        Task<bool> CheckIsLoggedInAsync();
    }
}
