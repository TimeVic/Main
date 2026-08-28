using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class SetTimeEntryEffect: Effect<SaveTimeEntryAction>
{
    private readonly IState<ProjectState> _projectState;
    private readonly IApiService _apiService;
    private readonly ILogger<SetTimeEntryEffect> _logger;
    private readonly IToastService _toastService;

    public SetTimeEntryEffect(
        IApiService apiService,
        IState<ProjectState> projectState,
        ILogger<SetTimeEntryEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
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
