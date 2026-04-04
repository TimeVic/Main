using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Manage;

public partial class EditTimeEntryModal: IDisposable
{
    [Parameter]
    public required TimeEntryDto Entry { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    private TimeEntryDto _model = new();
    private EditForm _form;
    private LumexModal modal;
    private EditContext _editContext;
    private bool _isActiveTimeEntry => _state.Value.ActiveEntry != null && _state.Value.ActiveEntry.Id == Entry.Id;
    
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);
        _model.UpdateFrom(Entry);
        await base.OnInitializedAsync();
        _editContext.OnFieldChanged += OnFormFieldChanged;
    }

    public void Dispose()
    {
        _editContext?.OnFieldChanged -= OnFormFieldChanged;
    }
    
    private void OnFormFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        
    }

    private async Task UpdateTimeEntry()
    {
        if (_editContext.Validate())
        {
            Dispatcher.Dispatch(new SaveTimeEntryAction(_model, true));
        }
        await Task.CompletedTask;
    }

    private void OnCloseModal()
    {
        OnClose.InvokeAsync();
    }

    private async Task OnProjectSelected(ProjectDto project)
    {
        _model.Project = project;
        await UpdateTimeEntry();
    }

    private async Task ClearProject()
    {
        _model.Project = null;
        await UpdateTimeEntry();
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
}
