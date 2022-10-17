using Fluxor;
using Radzen;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class AddEffect: Effect<SaveEmptyListItemAction>
{
    private readonly IState<WorkspaceState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly NotificationService _notificationService;

    public AddEffect(
        IApiService apiService,
        IState<WorkspaceState> state,
        ILogger<AddEffect> logger,
        NotificationService notificationService
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
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "New Workspace was added"
            });
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
