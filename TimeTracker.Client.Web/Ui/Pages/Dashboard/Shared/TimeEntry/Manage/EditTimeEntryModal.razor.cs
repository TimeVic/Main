using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.TimeEntry;
using TimeTracker.Client.Core.Services.DateTimes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Manage;

public partial class EditTimeEntryModal: IDisposable
{
    [Parameter]
    public required TimeEntryDto Entry { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = default!;
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; } = default!;
    
    [Inject]
    private UserDateTimeProviderService _dateTimeProviderService { get; set; } = default!;
    
    private TimeEntryDto _model = new();
    private EditContext _editContext = default!;
    private System.Timers.Timer? _timer;
    private bool _isAddTaskModalOpened;

    private bool IsActiveTimeEntry => _state.Value.ActiveEntry != null && _state.Value.ActiveEntry.Id == Entry.Id;

    private string TimeZoneId => _dateTimeProviderService.GetTimeZone().Id;
    
    private TimeSpan DisplayDuration
    {
        get
        {
            if (IsActiveTimeEntry)
                return _dateTimeProviderService.GetCurrentTime() - Entry.StartTimeOffset;
            return _model.Duration;
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);
        _model.UpdateFrom(Entry);
        _model.TimeZone = TimeZoneId;

        // Set interprets form values in the current workspace timezone.
        _model.StartTime = _dateTimeProviderService.ConvertUtcToWallClock(_model.StartTime, TimeZoneId);
        _model.EndTime = _dateTimeProviderService.ConvertUtcToWallClock(_model.EndTime, TimeZoneId);

        _editContext.OnFieldChanged += OnFormFieldChanged;
        await base.OnInitializedAsync();
        
        if (IsActiveTimeEntry)
        {
            _timer = new System.Timers.Timer(300);
            _timer.Elapsed += (_, _) => InvokeAsync(StateHasChanged);
            _timer.Start();
        }
    }

    public void Dispose()
    {
        _editContext.OnFieldChanged -= OnFormFieldChanged;
        _timer?.Dispose();
    }
    
    private void OnFormFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        
    }

    private async Task UpdateTimeEntry()
    {
        await UpdateTimeEntry(false);
    }

    private async Task UpdateTimeEntry(bool isSetProjectDefaults)
    {
        if (_editContext.Validate())
        {
            _model.EndTime = _model.EndTime == DateTime.MinValue ? null : _model.EndTime;
            Dispatcher.Dispatch(new SaveTimeEntryAction(_model, isSetProjectDefaults));
        }
        await Task.CompletedTask;
    }

    private void OnCloseModal()
    {
        OnClose.InvokeAsync();
    }

    private async Task OnProjectSelected(ProjectDto? project)
    {
        _model.Project = project;
        await UpdateTimeEntry(true);
    }

    private void OpenAddTaskModal()
    {
        if (_model.Project == null || _model.Task != null)
            return;
        _isAddTaskModalOpened = true;
    }

    private Task OnTaskAdded(TaskFullDto? task)
    {
        if (task == null)
            return Task.CompletedTask;

        _model.Task = task;
        _model.Project = task.TaskList.Project;
        Dispatcher.Dispatch(new SaveTimeEntryAction(_model, true));
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnChangeStartTime(DateTime? startTime)
    {
        if (startTime != null)
        {
            _model.StartTime = startTime > _model.EndTime ? _model.EndTime.Value : startTime.Value;
            await UpdateTimeEntry();
        }
    }

    private async Task OnChangeEndTime(DateTime? endTime)
    {
        if (endTime != null)
        {
            _model.EndTime = endTime < _model.StartTime ? _model.StartTime : endTime;
            await UpdateTimeEntry();
        }
    }

    private async Task OnDateChanged(DateTime? date)
    {
        if (!date.HasValue)
        {
            return;
        }

        _model.StartTime = _model.StartTime.WithDate(date.Value);
        _model.EndTime = _model.EndTime.WithDate(date.Value);

        if (_model.EndTime.HasValue)
        {
            _model.EndTime = _model.EndTime.Value < _model.StartTime ? _model.StartTime : _model.EndTime;
        }

        await UpdateTimeEntry();
    }

    private async Task OnDescriptionChanged(string description)
    {
        _model.Description = description;
        await UpdateTimeEntry();
    }

    private async Task OnChangeBillable(bool isBillable)
    {
        _model.IsBillable = isBillable;
        await UpdateTimeEntry();
    }

    private async Task OnChangeBillableAmount(decimal? hourlyRate)
    {
        _model.HourlyRate = hourlyRate;
        await UpdateTimeEntry();
    }

    private string GetTimeZoneLabel()
    {
        return TimeZoneId;
    }
}
