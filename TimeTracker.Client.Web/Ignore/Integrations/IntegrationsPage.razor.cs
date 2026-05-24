using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Web.Pages.Dashboard.Integrations;

public partial class IntegrationsPage
{
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private GetIntegrationSettingsResponse _settings { get; set; } = new ();
    private bool _isLoading { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _isLoading = true;
        _settings = await ApiService.WorkspaceIntegrationSettingsGetAsync(_authState.Value.Workspace.Id);
        _isLoading = false;
    }

    private string GetIcon(bool isActive)
    {
        return isActive ? MudBlazor.Icons.Material.Filled.Link : MudBlazor.Icons.Material.Filled.LinkOff;
    }
}
