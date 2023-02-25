using Fluxor;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly ApiService _apiService;
    private readonly IState<WorkspaceState> _state;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<WorkspaceState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
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
            var response = await _apiService.WorkspaceGetListAsync();
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
