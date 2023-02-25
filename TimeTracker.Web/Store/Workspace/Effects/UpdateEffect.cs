using Fluxor;
using Radzen;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Workspace.Effects;

public class UpdateEffect: Effect<UpdateWorkspaceAction>
{
    private readonly IState<WorkspaceState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly NotificationService _notificationService;

    public UpdateEffect(
        ApiService apiService,
        IState<WorkspaceState> state,
        ILogger<UpdateEffect> logger,
        NotificationService notificationService
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
            
            _notificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Info,
                Summary = "The workspace was updated"
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
