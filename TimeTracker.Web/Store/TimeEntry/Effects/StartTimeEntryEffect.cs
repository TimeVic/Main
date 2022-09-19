using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class StartTimeEntryEffect: Effect<StartTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _apiService.TimeEntryStartAsync(new StartRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
