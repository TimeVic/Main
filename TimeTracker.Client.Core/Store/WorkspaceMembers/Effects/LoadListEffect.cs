using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.WorkspaceMembers.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<WorkspaceMembersState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<WorkspaceMembersState> state,
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
        if (_authState.Value.Workspace?.Mode == WorkspaceMode.Solo)
        {
            dispatcher.Dispatch(
                new SetListItemsAction(new GetListResponse(new List<WorkspaceMemberDto>(), 0))
            );
            return;
        }

        try
        {
            var isLoad = action.IsReload || !action.IsReload && !_state.Value.IsLoaded;
            if (!isLoad)
            {
                return;
            }

            dispatcher.Dispatch(new SetIsListLoading(true));
            var response = await _apiService.WorkspaceMemberGetListAsync(new GetListRequest()
            {
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
