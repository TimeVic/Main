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
    private readonly ToastService _toastService;

    public SetTimeEntryEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<ProjectState> projectState,
        ILogger<SetTimeEntryEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _projectState = projectState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(SaveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            var project = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.TimeEntry.Project?.Id
            );
            var endTime = action.TimeEntry.EndTime == DateTime.MinValue
                ? null
                : action.TimeEntry.EndTime;
            
            var response = await _apiService.TimeEntrySetAsync(new SetRequest()
            {
                Id = action.TimeEntry.Id,
                Description = action.TimeEntry.Description,
                ProjectId = action.TimeEntry.Project?.Id,
                EndTime = endTime,
                StartTime = action.TimeEntry.StartTime,
                HourlyRate = action.TimeEntry.HourlyRate,
                IsBillable = action.IsSetProjectDefaults && project != null 
                    ? project.IsBillableByDefault 
                    : action.TimeEntry.IsBillable
            });
            dispatcher.Dispatch(new UpdateTimeEntryAction(response));
            _toastService.ShowInfo($"Time entry updated.");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
