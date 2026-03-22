using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntryCard
{
    [Parameter]
    public TimeEntryDto Entry { get; set; } = new();
    
    [Parameter]
    public EventCallback<TimeEntryDto> OnEdit { get; set; }
    
    [Parameter]
    public EventCallback<TimeEntryDto> OnDelete { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private string GetProjectLabel()
    {
        return string.IsNullOrWhiteSpace(Entry.Project?.Name) ? "No project" : Entry.Project?.Name ?? string.Empty;
    }

    private string GetTaskLabel()
    {
        return Entry.Task?.TaskId != null ? "No task" : Entry.Task?.TaskId.ToString() ?? string.Empty;
    }
}
