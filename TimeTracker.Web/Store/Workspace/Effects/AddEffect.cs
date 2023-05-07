using Fluxor;
using Radzen;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class AddEffect: Effect<SaveEmptyListItemAction>
{
    private readonly IState<WorkspaceState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly ToastService _notificationService;

    public AddEffect(
        ApiService apiService,
        IState<WorkspaceState> state,
        ILogger<AddEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _state = state;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(SaveEmptyListItemAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_state.Value.ItemToAdd == null)
            {
                return;
            }

            await _apiService.WorkspaceAddAsync(_state.Value.ItemToAdd.Name);
            dispatcher.Dispatch(new RemoveEmptyListItemAction());
            dispatcher.Dispatch(new LoadListAction(true));
            await _notificationService.ShowInfo("New Workspace was added");
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
