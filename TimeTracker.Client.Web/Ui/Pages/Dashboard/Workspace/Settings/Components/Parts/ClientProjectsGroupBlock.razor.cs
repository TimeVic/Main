using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Client;
using ClientUpdateAction = TimeTracker.Client.Core.Store.Client.UpdateAction;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

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

    private Task OnSaveClient()
    {
        if (Client != null && !string.IsNullOrWhiteSpace(Client.Name))
        {
            Dispatcher.Dispatch(new ClientUpdateAction(Client));
        }

        return Task.CompletedTask;
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
