using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Project;

public partial class ProjectPage
{
    [Parameter]
    public long ProjectId { get; set; }

    [Inject]
    private IState<ProjectState> _state { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    private ProjectDto? _project;
    
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        Dispatcher.Dispatch(new LoadProjectListAction());

        _project = _state.Value.List.FirstOrDefault(item => item.Id == ProjectId);
        if (_project == null)
        {
            _navigationManager.NavigateTo(SiteUrl.Dashboard_Projects);
        }
    }
}
