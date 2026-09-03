using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask.Components.Checklist;

public partial class ChecklistProgressBar
{
    [Parameter]
    public int TotalCount { get; set; }

    [Parameter]
    public int CompletedCount { get; set; }

    private int Percentage => TotalCount > 0
        ? (int)Math.Round((double)CompletedCount / TotalCount * 100)
        : 0;
}
