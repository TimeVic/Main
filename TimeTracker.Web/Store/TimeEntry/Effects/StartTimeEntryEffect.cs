using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class StartTimeEntryEffect: Effect<StartTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TimeEntryState> _timeEntryState;
    private readonly IApiService _apiService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<TimeEntryState> timeEntryState,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _timeEntryState = timeEntryState;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            if (_timeEntryState.Value.HasActiveEntry)
            {
                await _apiService.TimeEntryStopAsync(new StopRequest()
                {
                    WorkspaceId = _authState.Value.Workspace.Id,
                    EndTime = DateTime.Now.TimeOfDay,
                    EndDate = DateTime.Now
                });
                dispatcher.Dispatch(new LoadTimeEntryListAction(1));
            }
            
            var response = await _apiService.TimeEntryStartAsync(new StartRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Date = DateTime.UtcNow.Date,
                StartTime = DateTime.Now.TimeOfDay,
                
                TaskId = action.TaskId,
                IsBillable = action.IsBillable,
                ProjectId = action.Project?.Id,
                Description = action.Description,
                HourlyRate = action.HourlyRate
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
