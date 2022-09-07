using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RegistrationStep2ResponseDto : IResponse
    {
        public string JwtToken { get; set; }
        
        public UserDto User { get; set; }
    }
}
