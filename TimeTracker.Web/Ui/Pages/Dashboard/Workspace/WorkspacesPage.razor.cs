using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace;

public partial class WorkspacesPage
{
    [Inject]
    public IState<WorkspaceState> _state { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private Task OnAdd()
    {
        throw new NotImplementedException();
    }

    private Task OnEdit(WorkspaceDto context)
    {
        throw new NotImplementedException();
    }

    private Task OnDelete(WorkspaceDto context)
    {
        throw new NotImplementedException();
    }
}
