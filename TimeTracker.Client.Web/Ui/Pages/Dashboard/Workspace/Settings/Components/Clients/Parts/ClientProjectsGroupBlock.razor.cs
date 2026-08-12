using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients.Parts;

public partial class ClientProjectsGroupBlock
{
    [Parameter]
    public ClientDto? Client { get; set; }

    [Parameter]
    public IReadOnlyCollection<ProjectDto> Projects { get; set; } = [];

    [Parameter]
    public EventCallback<ClientDto?> AddProjectRequested { get; set; }

    [Parameter]
    public EventCallback<ProjectDto> EditProjectRequested { get; set; }

    [Parameter]
    public EventCallback<ClientDto> EditClientRequested { get; set; }

    private async Task OnEditClient()
    {
        if (Client != null)
        {
            await EditClientRequested.InvokeAsync(Client);
        }
    }

    private async Task OnAddProject()
    {
        await AddProjectRequested.InvokeAsync(Client);
    }

    private async Task OnEditProject(ProjectDto project)
    {
        await EditProjectRequested.InvokeAsync(project);
    }
}
