using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class DeleteTimeEntryEffect: Effect<DeleteTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteTimeEntryEffect> _logger;
    private readonly IToastService _toastService;

    public DeleteTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<DeleteTimeEntryEffect> logger,
        IToastService toastService
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
            _toastService.ShowInfo("Time entry deleted!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
