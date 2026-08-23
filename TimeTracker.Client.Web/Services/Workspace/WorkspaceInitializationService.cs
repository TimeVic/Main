using Fluxor;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.Dashboard;
using TimeTracker.Client.Core.Store.Permissions;
using TimeTracker.Client.Core.Store.TimeEntry;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Services.Workspace;

public class WorkspaceInitializationService
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<WorkspaceInitializationService> _logger;

    public WorkspaceInitializationService(
        IDispatcher dispatcher,
        IState<AuthState> authState,
        ApiService apiService,
        ILogger<WorkspaceInitializationService> logger
    )
    {
        _dispatcher = dispatcher;
        _authState = authState;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<bool> InitializeWorkspaceAsync(Guid? workspaceId = null)
    {
        try
        {
            _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(false));
            var response = await _apiService.DashboardInitAsync();
            if (response == null)
            {
                return false;
            }

            if (workspaceId.HasValue && response.CurrentWorkspace.Id != workspaceId.Value)
            {
                return false;
            }

            ApplyInitData(response);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to initialize workspace: {WorkspaceId}", workspaceId);
            return false;
        }
    }

    public void ApplyInitData(DashboardInitResponse response)
    {
        _dispatcher.Dispatch(new SetWorkspaceAction(response.CurrentWorkspace));
        _dispatcher.Dispatch(new UpdateUserAction(response.CurrentUser));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Workspace.SetListItemsAction(
            new PaginatedListDto<WorkspaceDto>(response.Workspaces, response.Workspaces.Count)
        ));
        _dispatcher.Dispatch(new SetWorkspacePermissionsAction(response.CurrentWorkspace.Id, response.Permissions));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.WorkspaceMembers.SetListItemsAction(
            new TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember.GetListResponse(
                response.WorkspaceMembers, response.WorkspaceMembers.Count
            )
        ));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetListItemsAction(
            new TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListResponse(
                response.Projects, response.Projects.Count
            )
        ));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Client.SetListItemsAction(
            new TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.GetListResponse(
                response.Clients, response.Clients.Count
            )
        ));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tag.SetListItemsAction(
            new TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag.GetListResponse(
                response.Tags, response.Tags.Count
            )
        ));
        _dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetListItemsAction(
            new TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListResponse(
                response.TaskLists, response.TaskLists.Count
            )
        ));
        _dispatcher.Dispatch(new SetActiveTimeEntryAction(response.ActiveTimeEntry));
        _dispatcher.Dispatch(new FetchCountersAction());
        _dispatcher.Dispatch(new SetIsWorkspaceInitializedAction(true));
    }

    public async Task<bool> EnsureWorkspaceAsync(Guid? workspaceId)
    {
        return await InitializeWorkspaceAsync(workspaceId);
    }

    public void Init(bool isReload = false)
    {
        // Handled as part of InitializeWorkspaceAsync. Kept for backwards compatibility.
    }

    public async Task AfterInit(bool isReload = false)
    {
        // Handled as part of InitializeWorkspaceAsync. Kept for backwards compatibility.
        await Task.CompletedTask;
    }

    public void ChangeWorkspace(WorkspaceDto workspace, string? destinationPath = null)
    {
        if (workspace.Id == _authState.Value.Workspace?.Id)
        {
            return;
        }
        _dispatcher.Dispatch(new SelectWorkspaceAction(workspace, destinationPath));
    }
}
