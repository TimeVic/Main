using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class StartTimeEntryEffect: Effect<StartTimeEntryAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TimeEntryState> _timeEntryState;
    private readonly IState<ProjectState> _projectState;
    private readonly IApiService _apiService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<TimeEntryState> timeEntryState,
        IState<ProjectState> projectState,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _timeEntryState = timeEntryState;
        _projectState = projectState;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessing(true));
            if (_timeEntryState.Value.HasActiveEntry)
            {
                await _apiService.TimeEntryStopAsync(new StopRequest()
                {
                    WorkspaceId = _authState.Value.Workspace.Id,
                    EndTime = DateTime.Now.TimeOfDay,
                    EndDate = DateTime.Now
                });
                dispatcher.Dispatch(new LoadListAction(1));
            }

            var project = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.Project?.Id
            );
            if (action.InternalTask != null)
            {
                project = action.InternalTask?.TaskList.Project;
            }
            var response = await _apiService.TimeEntryStartAsync(new StartRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Date = DateTime.Now.ToDateAndRemoveTimeZone(),
                StartTime = DateTime.Now.TimeOfDay,

                TaskId = action.TaskId,
                IsBillable = project != null ? project.IsBillableByDefault : action.IsBillable,
                ProjectId = action.Project?.Id,
                Description = action.Description,
                HourlyRate = action.HourlyRate,
                InternalTaskId = action.InternalTask?.Id
            });
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessing(false));
        }
    }
}
