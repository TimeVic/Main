using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class SetTimeEntryEffect: Effect<SaveTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<ProjectState> _projectState;
    private readonly ApiService _apiService;
    private readonly ILogger<SetTimeEntryEffect> _logger;

    public SetTimeEntryEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<ProjectState> projectState,
        ILogger<SetTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _projectState = projectState;
        _logger = logger;
    }

    public override async Task HandleAsync(SaveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            var project = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.TimeEntry.Project?.Id
            );
            
            var response = await _apiService.TimeEntrySetAsync(new SetRequest()
            {
                Id = action.TimeEntry.Id,
                WorkspaceId = _authState.Value.Workspace.Id,
                Description = action.TimeEntry.Description,
                Date = action.TimeEntry.Date,
                ProjectId = action.TimeEntry.Project?.Id,
                EndTime = action.TimeEntry.EndTime,
                StartTime = action.TimeEntry.StartTime,
                HourlyRate = action.TimeEntry.HourlyRate,
                IsBillable = action.IsSetProjectDefaults && project != null 
                    ? project.IsBillableByDefault 
                    : action.TimeEntry.IsBillable
            });
            dispatcher.Dispatch(new UpdateTimeEntryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
