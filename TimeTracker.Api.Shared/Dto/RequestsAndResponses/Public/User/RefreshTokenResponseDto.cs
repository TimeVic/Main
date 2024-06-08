using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RefreshTokenResponseDto : IResponse
    {
        public string JwtToken { get; set; } = "";
        
        public string AccessToken { get; set; } = "";
    }
}
