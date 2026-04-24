using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TasksList.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            if (!action.ProjectId.HasValue)
            {
                dispatcher.Dispatch(new SetListItemsAction(new GetListResponse(new List<TaskListDto>(), 0)));
                return;
            }

            dispatcher.Dispatch(new SetIsListLoadingAction(true));
            var response = await _apiService.TaskListGetListAsync(new GetListRequest()
            {
                WorkspaceId = _authState.Value.Workspace!.Id,
                ProjectId = action.ProjectId.Value
            });
            if (response == null)
            {
                dispatcher.Dispatch(new SetListItemsAction(new GetListResponse(new List<TaskListDto>(), 0), action.ProjectId));
                return;
            }

            dispatcher.Dispatch(new SetListItemsAction(response, action.ProjectId));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoadingAction(false));
        }
    }
}
