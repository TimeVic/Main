using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class LoginResponseDto : IResponse
    {
        public string Token { get; set; }
        
        public string PrivateKeyBase64 { get; set; }
    }
}
