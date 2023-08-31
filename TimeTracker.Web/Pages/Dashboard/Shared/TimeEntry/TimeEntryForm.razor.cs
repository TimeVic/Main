using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryForm
{
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public bool IsShort { get; set; }

    [Parameter]
    public bool IsShowTimeEntriesButton { get; set; } = false;
    
    [Parameter]
    public TaskDto? InternalTask { get; set; }

    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject] 
    private ModalDialogProviderService _modalDialogProviderService { get; set; }

    private TimeEntryDto? _activeEntry
    {
        get
        {
            if (InternalTask == null)
                return _state.Value.ActiveEntry;
            if (InternalTask?.TaskId == _state.Value.ActiveEntry?.Task?.TaskId)
            {
                return _state.Value.ActiveEntry;
            }
            // If displayed in other tasks
            return null;
        }
    }
    
    private bool _hasActiveEntry
    {
        get
        {
            if (InternalTask == null)
                return _state.Value.HasActiveEntry;
            if (InternalTask?.TaskId == _state.Value.ActiveEntry?.Task?.TaskId)
            {
                return _state.Value.HasActiveEntry;
            }
            // If displayed in other tasks
            return false;
        }
    }

    private void ToggleTimeEntry(bool isStarted)
    {
        if (isStarted)
        {
            Dispatcher.Dispatch(new StartTimeEntryAction(InternalTask: InternalTask));
        }
        else
        {
            Dispatcher.Dispatch(new StopActiveTimeEntryAction());
        }
    }
    
    private async Task OnChangeDescription(string? value)
    {
        _activeEntry.Description = value;
        await UpdateTimeEntry(_activeEntry);
        await Task.CompletedTask;
    }
    
    private async Task OnChangeProject(ProjectDto project)
    {
        _activeEntry.Project = project;
        await UpdateTimeEntry(_activeEntry);
        await Task.CompletedTask;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, true));
        await Task.CompletedTask;
    }
    
    private async Task ShowAddTaskModal(long timEntryId)
    {
        await _modalDialogProviderService.ShowAddTaskModal(timEntryId);
    }
    
    private async Task ShowTimeEntriesModal()
    {
        await _modalDialogProviderService.ShowTimeEntriesModal();
    }
    
    private string GetDescriptionLabel(TimeEntryDto? timeEntry)
    {
        if (timeEntry?.Task != null)
        {
            return timeEntry.Task.Title.TruncateAndAddDots(20);
        }
        return "Description";
    }
}
