using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntryCard
{
    [Parameter]
    public TimeEntryDto Entry { get; set; } = new();
    
    [Parameter]
    public EventCallback<TimeEntryDto> OnEdit { get; set; }
    
    [Parameter]
    public EventCallback<TimeEntryDto> OnDelete { get; set; }
    
    [Parameter]
    public EventCallback<TimeEntryDto> OnClone { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private string GetProjectLabel()
    {
        return string.IsNullOrWhiteSpace(Entry.Project?.Name) ? "No project" : Entry.Project?.Name ?? "No project";
    }

    private string GetTaskLabel()
    {
        return Entry.Task?.TaskId != null ? "No task" : Entry.Task?.TaskId.ToString() ?? "No task";
    }
}
