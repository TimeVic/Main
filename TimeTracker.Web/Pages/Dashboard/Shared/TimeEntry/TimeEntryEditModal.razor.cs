using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryEditModal
{
    [Parameter]
    public TimeEntryDto TimeEntry { get; set; }

    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private MudForm? _form;
    private bool _isValid = false;
    public TimeSpan _duration => TimeEntry.EndTime == null ? TimeSpan.Zero : TimeEntry.EndTime.Value - TimeEntry.StartTime;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private void OnChangeStartTime(TimeSpan startTime)
    {
        TimeEntry.StartTime = startTime > TimeEntry.EndTime ? TimeEntry.EndTime.Value : startTime;
    }

    private void OnChangeEndTime(TimeSpan endTime)
    {
        TimeEntry.EndTime = endTime < TimeEntry.StartTime ? TimeEntry.StartTime : endTime;
    }
    
    private void SubmitForm()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        OnCloseModal();
        Dispatcher.Dispatch(new UpdateTimeEntryAction(TimeEntry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(TimeEntry));
    }
    
    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
