using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.Integrations;

public class MobileSignInResponse: IResponse
{
    public string? AccessToken { get; set; }
    
    public string? JwtToken { get; set; }
}
