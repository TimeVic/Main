using Fluxor;
using Radzen;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class UpdateEffect: Effect<UpdateWorkspaceAction>
{
    private readonly IState<WorkspaceState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly ToastService _notificationService;

    public UpdateEffect(
        ApiService apiService,
        IState<WorkspaceState> state,
        ILogger<UpdateEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _state = state;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(UpdateWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.WorkspaceUpdateAsync(
                action.Workspace.Id,
                action.Workspace.Name
            );
            dispatcher.Dispatch(new LoadListAction(true));
            
            await _notificationService.ShowInfo("The workspace was updated");
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
