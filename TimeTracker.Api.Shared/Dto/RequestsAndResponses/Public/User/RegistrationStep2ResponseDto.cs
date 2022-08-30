using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RegistrationStep2ResponseDto : IResponse
    {
        public string PrivateKeyBase64 { get; set; }
        public string Pem { get; set; }
        public string JwtToken { get; set; }
    }
}
