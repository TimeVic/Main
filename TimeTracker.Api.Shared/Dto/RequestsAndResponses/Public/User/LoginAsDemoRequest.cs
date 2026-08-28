using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

public class LoginAsDemoRequest : IRequest<LoginResponseDto>
{
    public WorkspaceMode? Mode { get; set; }
}


