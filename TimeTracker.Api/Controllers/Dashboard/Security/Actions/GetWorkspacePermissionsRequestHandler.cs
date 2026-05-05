using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Security;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.Security.Actions;

public class GetWorkspacePermissionsRequestHandler : IAsyncRequestHandler<GetWorkspacePermissionsRequest, GetWorkspacePermissionsResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IClientPermissionService _clientPermissionService;

    public GetWorkspacePermissionsRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IClientPermissionService clientPermissionService
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _clientPermissionService = clientPermissionService;
    }

    public async Task<GetWorkspacePermissionsResponse> ExecuteAsync(GetWorkspacePermissionsRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
        RecordNotFoundException.ThrowIfNull(workspace);

        return new GetWorkspacePermissionsResponse
        {
            WorkspaceId = workspace.Id,
            Permissions = await _clientPermissionService.GetPermissionsAsync(user, workspace)
        };
    }
}
