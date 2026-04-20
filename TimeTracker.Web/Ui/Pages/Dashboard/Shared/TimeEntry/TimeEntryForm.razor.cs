using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.TimeEntry;
// using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.TimeEntry;

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
    
    private bool _isEditModalOpened = false;
    private bool _isDetailsOpened = false;
    
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
    
    private async Task OnChangeDescription(string? value)
    {
        _activeEntry!.Description = value;
        await UpdateTimeEntry(_activeEntry);
        await Task.CompletedTask;
    }
    
    private async Task OnChangeProject(ProjectDto project)
    {
        _activeEntry!.Project = project;
        await UpdateTimeEntry(_activeEntry);
        await Task.CompletedTask;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, true));
        await Task.CompletedTask;
    }
    
    private async Task ShowAddTaskModal(Guid timEntryId)
    {
        // await _modalDialogProviderService.ShowAddTaskModal(timEntryId);
    }
    
    private async Task ShowTimeEntriesModal()
    {
        // await _modalDialogProviderService.ShowTimeEntriesModal();
    }
    
    private string GetDescriptionLabel(TimeEntryDto? timeEntry)
    {
        if (timeEntry?.Task != null)
        {
            return timeEntry.Task.Title.TruncateAndAddDots(20);
        }
        return "Description";
    }

    private string GetTrackingContext()
    {
        if (_activeEntry == null)
        {
            return string.Empty;
        }

        var taskName = !string.IsNullOrWhiteSpace(_activeEntry.Task?.Title)
            ? _activeEntry.Task.Title
            : !string.IsNullOrWhiteSpace(_activeEntry.Description)
                ? _activeEntry.Description
                : "Time entry";
        var projectName = _activeEntry.Project?.Name ?? _activeEntry.Task?.TaskList.Project.Name ?? "No project";
        var clientName = _activeEntry.Project?.Client?.Name ?? _activeEntry.Task?.TaskList.Project.Client?.Name;
        var clientPart = string.IsNullOrWhiteSpace(clientName) ? string.Empty : $"Client: {clientName}, ";
        var ratePart = _activeEntry.HourlyRate.HasValue
            ? $", Rate: {FormatMoney(_activeEntry.HourlyRate.Value)}/h"
            : string.Empty;

        return $"{taskName} ({clientPart}Project: {projectName}{ratePart})";
    }

    private string FormatMoney(decimal amount)
    {
        return AuthState.Value.Workspace != null
            ? $"{amount.ToMoneyFormat()} {AuthState.Value.Workspace.Currency.Symbol}"
            : amount.ToMoneyFormat();
    }
}
