using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryForm : IDisposable
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
    
    private bool _isEditModalOpened = false;
    private bool _isAddTaskModalOpened = false;
    private bool _isUpdateTaskModalOpened = false;
    
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _state.StateChanged += OnTimeEntryStateChanged;
    }

    private void ToggleTimeEntry()
    {
        if (_hasActiveEntry)
        {
            Dispatcher.Dispatch(new StopActiveTimeEntryAction());
        }
        else
        {
            Dispatcher.Dispatch(new StartTimeEntryAction(InternalTask: InternalTask));
        }
    }

    private void OnDescriptionClick()
    {
        if (!_hasActiveEntry)
            ToggleTimeEntry();
    }

    private void OnProjectAreaClick()
    {
        if (!_hasActiveEntry)
            ToggleTimeEntry();
    }
    
    private async Task OnChangeProject(ProjectDto? project)
    {
        if (!_hasActiveEntry || _activeEntry == null || _activeEntry.Task != null)
        {
            return;
        }

        _activeEntry!.Project = project;
        await UpdateTimeEntry(_activeEntry, true);
        await Task.CompletedTask;
    }

    private async Task OnDescriptionChanged(ChangeEventArgs e)
    {
        if (_activeEntry == null)
            return;

        _activeEntry.Description = e.Value?.ToString();
        await UpdateTimeEntry(_activeEntry);
    }

    private void OpenAddTaskModal()
    {
        if (!_hasActiveEntry || _activeEntry?.Project == null || _activeEntry.Task != null)
        {
            return;
        }

        _isAddTaskModalOpened = true;
    }

    private Task OnTaskAdded(TaskFullDto? task)
    {
        if (task == null || _activeEntry == null)
        {
            return Task.CompletedTask;
        }

        _activeEntry.Task = task;
        _activeEntry.Project = task.TaskList.Project;
        Dispatcher.Dispatch(new UpdateTimeEntryAction(_activeEntry));
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry, bool isSetProjectDefaults = false)
    {
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, isSetProjectDefaults));
        await Task.CompletedTask;
    }

    private void OnTimeEntryStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _state.StateChanged -= OnTimeEntryStateChanged;
    }
}
