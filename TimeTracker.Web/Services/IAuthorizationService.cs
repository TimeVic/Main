using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Web.Services
{
    public interface IAuthorizationService
    {
        Task<bool> LoginAsync(LoginRequest model);
        void Login(string jwtToken, UserDto user);
        Task LogoutAsync();
        Task<bool> CheckIsLoggedInAsync();
        string GetJwt();
    }
}
