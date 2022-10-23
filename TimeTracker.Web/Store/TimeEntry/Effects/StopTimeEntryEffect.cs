using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class StopTimeEntryEffect: Effect<StopActiveTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<StopTimeEntryEffect> _logger;

    public StopTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<StopTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(StopActiveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.TimeEntryStopAsync(new StopRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                EndTime = DateTime.Now.TimeOfDay
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(null));
            dispatcher.Dispatch(new LoadTimeEntryListAction(1));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
