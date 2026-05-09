using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;

namespace TimeTracker.Web.Services.Http;

public partial class ApiService
{
    public async Task<GetWorkspacePermissionsResponse?> GetWorkspacePermissionsAsync(Guid workspaceId)
    {
        return await PostAsync<GetWorkspacePermissionsResponse>(
            ApiUrl.WorkspacePermissions,
            new GetWorkspacePermissionsRequest
            {
            }
        );
    }
}
