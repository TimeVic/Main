using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long? ClientId { get; set; }

    [Inject]
    public IState<ProjectState> ProjectState { get; set; }

    public ICollection<ProjectDto> Projects
    {
        get
        {
            var projects = ProjectState.Value.SortedList;
            return projects.Where(item => item.Client?.Id == ClientId).ToList();
        }
    }
}
