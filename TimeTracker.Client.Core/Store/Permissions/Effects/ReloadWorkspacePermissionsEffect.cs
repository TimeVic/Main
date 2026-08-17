using Fluxor;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Permissions.Effects;

public class ReloadWorkspacePermissionsEffect : Effect<ReloadWorkspacePermissionsAction>
{
    private readonly IApiService _apiService;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<ReloadWorkspacePermissionsEffect> _logger;

    public ReloadWorkspacePermissionsEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<ReloadWorkspacePermissionsEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(ReloadWorkspacePermissionsAction action, IDispatcher dispatcher)
    {
        var workspaceId = _authState.Value.Workspace?.Id;
        if (!workspaceId.HasValue)
        {
            dispatcher.Dispatch(new ClearWorkspacePermissionsAction());
            action.CompletionSource?.TrySetResult();
            return;
        }

        try
        {
            dispatcher.Dispatch(new SetWorkspacePermissionsLoadingAction(true));
            var response = await _apiService.GetWorkspacePermissionsAsync(workspaceId.Value);
            dispatcher.Dispatch(
                new SetWorkspacePermissionsAction(
                    response ?? new GetWorkspacePermissionsResponse
                    {
                        Permissions = new List<WorkspacePermission>()
                    }
                )
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load workspace permissions.");
            dispatcher.Dispatch(
                new SetWorkspacePermissionsAction(
                    new GetWorkspacePermissionsResponse
                    {
                        Permissions = new List<WorkspacePermission>()
                    }
                )
            );
        }
        finally
        {
            dispatcher.Dispatch(new SetWorkspacePermissionsLoadingAction(false));
            action.CompletionSource?.TrySetResult();
        }
    }
}
