using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class CheckLoginResponse : IResponse
{
    public bool IsAvailable { get; set; }
}
