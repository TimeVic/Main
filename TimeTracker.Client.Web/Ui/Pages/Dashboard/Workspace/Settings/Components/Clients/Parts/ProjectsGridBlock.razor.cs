using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients.Parts;

public partial class ProjectsGridBlock
{
    [Parameter]
    public IReadOnlyCollection<ProjectDto> Projects { get; set; } = [];

    [Parameter]
    public EventCallback<ProjectDto> EditRequested { get; set; }

    [Inject]
    private IState<AuthState> AuthState { get; set; } = default!;

    private string GetFormattedRate(decimal? rate)
    {
        if (!rate.HasValue || rate.Value <= 0)
        {
            return DashboardLocalizer["ClientProjectsGroupBlock_NoRate"];
        }

        var currency = AuthState.Value.Workspace?.Currency.Code ?? "USD";
        return $"{rate.Value:0.00} {currency}";
    }

    private async Task OnEdit(ProjectDto project)
    {
        await EditRequested.InvokeAsync(project);
    }
}
