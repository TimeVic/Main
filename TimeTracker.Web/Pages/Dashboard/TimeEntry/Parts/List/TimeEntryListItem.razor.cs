using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Components;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.List;

public partial class TimeEntryListItem
{
    [Parameter]
    public TimeEntryDto Item { get; set; }

    private async Task OnChangeStartTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }
}
