using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.TimeEntryForm;

public partial class TimeEntryForm
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }

    private void StartTimeEntry()
    {
        Dispatcher.Dispatch(new StartTimeEntryAction());
    }

    private void StopTimeEntry()
    {
        Dispatcher.Dispatch(new StopActiveTimeEntryAction());
    }
    
    private async Task OnChangeStartTime(TimeSpan startTime)
    {
        
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }

    private async Task OnChangeTaskId(string value)
    {
        _state.Value.ActiveEntry.TaskId = value;
        await UpdateTimeEntry(_state.Value.ActiveEntry);
        await Task.CompletedTask;
    }
    
    private async Task OnChangeDescription(string value)
    {
        _state.Value.ActiveEntry.Description = value;
        await UpdateTimeEntry(_state.Value.ActiveEntry);
        await Task.CompletedTask;
    }
    
    private async Task OnChangeProject(ProjectDto project)
    {
        _state.Value.ActiveEntry.Project = project;
        await UpdateTimeEntry(_state.Value.ActiveEntry);
        await Task.CompletedTask;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, true));
        await Task.CompletedTask;
    }
}
