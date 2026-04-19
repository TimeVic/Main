using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;

public class GetWorkspacePermissionsResponse : IResponse
{
    public Guid WorkspaceId { get; set; }

    public ICollection<WorkspacePermission> Permissions { get; set; } = new List<WorkspacePermission>();
}
