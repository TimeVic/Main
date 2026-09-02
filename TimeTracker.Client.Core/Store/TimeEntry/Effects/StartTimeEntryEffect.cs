using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class StartTimeEntryEffect: Effect<StartTimeEntryAction>
{
    private readonly IState<ProjectState> _projectState;
    private readonly IApiService _apiService;
    private readonly NavigationManager _navigationManager;
    private readonly UrlService _urlService;
    private readonly ILogger<StartTimeEntryEffect> _logger;

    public StartTimeEntryEffect(
        IApiService apiService,
        IState<ProjectState> projectState,
        NavigationManager navigationManager,
        UrlService urlService,
        ILogger<StartTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _projectState = projectState;
        _navigationManager = navigationManager;
        _urlService = urlService;
        _logger = logger;
    }

    public override async Task HandleAsync(StartTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            var project = _projectState.Value.List.FirstOrDefault(
                item => item.Id == action.Project?.Id
            );
            if (action.InternalTask != null)
            {
                project = action.InternalTask?.TaskList.Project;
            }
            var response = await _apiService.TimeEntryStartAsync(new StartRequest()
            {
                IsBillable = project != null ? project.IsBillableByDefault : action.IsBillable,
                ProjectId = action.Project?.Id,
                Description = action.Description,
                HourlyRate = action.HourlyRate,
                InternalTaskId = action.InternalTask?.Id
            });

            var tasksToUpdate = new List<TaskDto>();
            if (response?.StoppedTimeEntry?.Task != null)
            {
                tasksToUpdate.Add(response.StoppedTimeEntry.Task);
            }
            if (response?.ActiveTimeEntry?.Task != null)
            {
                tasksToUpdate.Add(response.ActiveTimeEntry.Task);
            }
            if (tasksToUpdate.Any())
            {
                dispatcher.Dispatch(new UpdateListItemsAction(tasksToUpdate));
            }

            AddStoppedTimeEntryToListIfTimeEntriesPageIsOpen(response?.StoppedTimeEntry, dispatcher);
            dispatcher.Dispatch(new SetActiveTimeEntryAction(response?.ActiveTimeEntry));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(false));
        }
    }

    private void AddStoppedTimeEntryToListIfTimeEntriesPageIsOpen(
        TimeEntryDto? stoppedTimeEntry,
        IDispatcher dispatcher
    )
    {
        if (stoppedTimeEntry == null)
        {
            return;
        }

        var currentPath = _navigationManager.GetPath().TrimEnd('/');
        var timeEntriesPath = _urlService.GetDashboardUrl().TrimEnd('/');
        if (!string.Equals(currentPath, timeEntriesPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        dispatcher.Dispatch(new AddTimeEntryToListAction(stoppedTimeEntry));
    }
}
