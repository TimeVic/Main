using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;

public partial class TaskDetailEmptyState
{
    [Parameter]
    public required string Title { get; set; }
}
