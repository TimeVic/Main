using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long ProjectId { get; set; }

    [Parameter]
    public long? TaskListId { get; set; }
}
