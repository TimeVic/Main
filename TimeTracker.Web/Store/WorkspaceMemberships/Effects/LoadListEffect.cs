using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMemberships.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembershipsState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembershipsState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            var isLoad = action.IsReload || !action.IsReload && !_state.Value.IsLoaded;
            if (!isLoad)
            {
                return;
            }

            dispatcher.Dispatch(new SetIsListLoading(true));
            var response = await _apiService.WorkspaceMembershipGetListAsync(new GetListRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Page = 1
            });
            dispatcher.Dispatch(new SetListItemsAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
