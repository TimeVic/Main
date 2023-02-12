using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long? ClientId { get; set; }
}
