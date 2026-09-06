using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Services.DateTimes;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.TimeEntry;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Manage;

public partial class EditTimeEntryModal: IDisposable
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public TimeEntryDto? Entry { get; set; }

    [Parameter]
    public TimeEntryDto? TimeEntry { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = default!;
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; } = default!;
    
    [Inject]
    private UserDateTimeProviderService _dateTimeProviderService { get; set; } = default!;

    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;
    
    private TimeEntryDto _model = new();
    private EditContext _editContext = default!;
    private System.Timers.Timer? _timer;

    private TimeEntryDto ResolvedEntry => TimeEntry ?? Entry ?? _model;

    private bool IsActiveTimeEntry => _state.Value.ActiveEntry != null && _state.Value.ActiveEntry.Id == ResolvedEntry.Id;

    private string TimeZoneId => _dateTimeProviderService.GetTimeZone().Id;
    
    private TimeSpan DisplayDuration
    {
        get
        {
            if (IsActiveTimeEntry)
                return _dateTimeProviderService.GetCurrentTime() - ResolvedEntry.StartTimeOffset;
            return _model.Duration;
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);
        _model.UpdateFrom(ResolvedEntry);
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

    protected override void OnParametersSet()
    {
        if ((TimeEntry != null || Entry != null) && _model.Id != ResolvedEntry.Id)
        {
            _model.UpdateFrom(ResolvedEntry);
            _model.TimeZone = TimeZoneId;
            _model.StartTime = _dateTimeProviderService.ConvertUtcToWallClock(_model.StartTime, TimeZoneId);
            _model.EndTime = _dateTimeProviderService.ConvertUtcToWallClock(_model.EndTime, TimeZoneId);
        }
        base.OnParametersSet();
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

    private async Task OnCloseModal()
    {
        await OnClose.InvokeAsync();
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
    }

    private async Task OnProjectSelected(ProjectDto? project)
    {
        _model.Project = project;
        await UpdateTimeEntry(true);
    }

    private async Task OpenAddTaskModal()
    {
        if (_model.Project == null || _model.Task != null)
            return;
        await _modalDialogService.ShowAddTaskModal(
            projectId: _model.Project.Id,
            timeEntryId: _model.Id,
            onClose: res =>
            {
                if (res.Data is TaskFullDto task)
                {
                    OnTaskAdded(task);
                }
            }
        );
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
            var newStartTime = startTime > _model.EndTime ? _model.EndTime.Value : startTime.Value;
            var isChanged = _model.StartTime.Date != newStartTime.Date 
                            || _model.StartTime.Hour != newStartTime.Hour 
                            || _model.StartTime.Minute != newStartTime.Minute;
            if (isChanged)
            {
                _model.StartTime = newStartTime;
                await UpdateTimeEntry();
            }
        }
    }

    private async Task OnChangeEndTime(DateTime? endTime)
    {
        if (endTime.HasValue)
        {
            var newEndTime = endTime.Value < _model.StartTime ? _model.StartTime : endTime.Value;
            var isChanged = !_model.EndTime.HasValue 
                            || _model.EndTime.Value.Date != newEndTime.Date 
                            || _model.EndTime.Value.Hour != newEndTime.Hour 
                            || _model.EndTime.Value.Minute != newEndTime.Minute;
            if (isChanged)
            {
                _model.EndTime = newEndTime;
                await UpdateTimeEntry();
            }
        }
    }

    private async Task OnDateChanged(DateTime? date)
    {
        if (!date.HasValue || date.Value.Date == _model.StartTime.Date)
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
        if (_model.Description != description)
        {
            _model.Description = description;
            await UpdateTimeEntry();
        }
    }

    private async Task OnChangeBillable(bool isBillable)
    {
        if (_model.IsBillable != isBillable)
        {
            _model.IsBillable = isBillable;
            await UpdateTimeEntry();
        }
    }

    private async Task OnChangeBillableAmount(decimal? hourlyRate)
    {
        if (_model.HourlyRate != hourlyRate)
        {
            _model.HourlyRate = hourlyRate;
            await UpdateTimeEntry();
        }
    }

    private string GetTimeZoneLabel()
    {
        return TimeZoneId;
    }
}
