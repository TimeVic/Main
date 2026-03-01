using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;
using Debug = TimeTracker.Web.Core.Helpers.Debug;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryEditModal
{
    public class Parameters
    {
        public required TimeEntryDto TimeEntry { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }
    
    [CascadingParameter] 
    public required FluentDialog MudDialog { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private EditForm? _form;
    private bool _isValid = false;
    public TimeSpan _duration => Content.TimeEntry.EndTime == null ? TimeSpan.Zero : Content.TimeEntry.EndTime.Value - Content.TimeEntry.StartTime;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private void OnChangeStartTime(TimeSpan? startTime)
    {
        if (startTime != null)
        {
            Content.TimeEntry.StartTime = startTime > Content.TimeEntry.EndTime ? Content.TimeEntry.EndTime.Value : startTime.Value;
            SubmitForm();
        }
    }

    private void OnChangeEndTime(TimeSpan? endTime)
    {
        if (endTime != null)
        {
            Content.TimeEntry.EndTime = endTime < Content.TimeEntry.StartTime ? Content.TimeEntry.StartTime : endTime;
            SubmitForm();
        }
    }
    
    private void SubmitForm()
    {
        ArgumentNullException.ThrowIfNull(_form);
        ArgumentNullException.ThrowIfNull(_form.EditContext);
        
        if (!_form.EditContext.Validate())
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateTimeEntryAction(Content.TimeEntry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(Content.TimeEntry));
    }
    
    private void OnCloseModal()
    {
        MudDialog.CloseAsync();
    }

    private void OnChangeDescription(string? description)
    {
        Content.TimeEntry.Description = description;
        SubmitForm();
    }

    private void OnDateChanged(DateTime? date)
    {
        ArgumentNullException.ThrowIfNull(date);
        Content.TimeEntry.Date = DateOnly.FromDateTime(date.Value.Date);
        SubmitForm();
    }

    private void OnProjectChanged(ProjectDto? project)
    {
        Content.TimeEntry.Project = project;
        SubmitForm();
    }
}
