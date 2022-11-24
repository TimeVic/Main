using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Integrations;

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
}
