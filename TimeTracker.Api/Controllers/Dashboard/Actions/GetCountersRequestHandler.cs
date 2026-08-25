using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Counters;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Dashboard;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Actions;

public class GetCountersRequestHandler : IAsyncRequestHandler<GetCountersRequest, GetCountersResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IDashboardDao _dashboardDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetCountersRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IDashboardDao dashboardDao,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _dashboardDao = dashboardDao;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<GetCountersResponse> ExecuteAsync(GetCountersRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspaceId = _apiRequestService.GetCurrentWorkspaceId();
        var workspace = workspaceId.HasValue
            ? await _userDao.GetUsersWorkspace(user, workspaceId.Value)
            : await _userDao.GetDefaultWorkspace(user);
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        DashboardCountersDto counters;
        if (userAccess is MembershipAccessType.Owner or MembershipAccessType.Manager
            && workspace.Mode == WorkspaceMode.Team)
        {
            counters = await _dashboardDao.GetCountersAsync(workspace);
        }
        else
        {
            counters = new DashboardCountersDto { PendingApprovalsCount = 0 };
        }

        return new GetCountersResponse
        {
            Counters = counters
        };
    }
}
