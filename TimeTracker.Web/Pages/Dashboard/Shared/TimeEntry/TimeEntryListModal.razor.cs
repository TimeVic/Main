using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryListModal
{
    [Parameter]
    public bool IsFilteredList { get; set; } = false;
}
