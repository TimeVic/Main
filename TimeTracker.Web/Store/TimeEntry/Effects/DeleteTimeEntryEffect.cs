using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class DeleteTimeEntryEffect: Effect<DeleteTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ILogger<DeleteTimeEntryEffect> _logger;
    private readonly ToastService _toastService;

    public DeleteTimeEntryEffect(
        ApiService apiService,
        IState<AuthState> authState,
        ILogger<DeleteTimeEntryEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(DeleteTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.TimeEntryDeleteAsync(action.EntryId);
            dispatcher.Dispatch(new DeleteTimeEntryFromListAction(action.EntryId));
            await _toastService.ShowInfo("Time entry deleted!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
