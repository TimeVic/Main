using Fluxor;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class DeleteTimeEntryEffect: Effect<DeleteTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteTimeEntryEffect> _logger;
    private readonly NotificationService _toastService;

    public DeleteTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<DeleteTimeEntryEffect> logger,
        NotificationService toastService
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
            _toastService.Notify(new NotificationMessage()
            {
                Summary = "Time entry deleted!",
                Severity = NotificationSeverity.Info,
                
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
