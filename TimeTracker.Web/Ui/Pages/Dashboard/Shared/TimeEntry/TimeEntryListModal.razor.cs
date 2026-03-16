using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryListModal
{
    public class Parameters
    {
        public bool IsFilteredList { get; set; } = false;
    }

    [Parameter]
    public required Parameters Content { get; set; }
}
