using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class LoginResponseDto : IResponse
    {
        public string Token { get; set; }
        
        public UserDto User { get; set; }
    }
}
