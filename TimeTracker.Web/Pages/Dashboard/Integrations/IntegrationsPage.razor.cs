using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;

namespace TimeTracker.Web.Pages.Dashboard.Integrations;

public partial class IntegrationsPage
{
    private GetIntegrationSettingsResponse _settings { get; set; } = new ();
    private bool _isLoading { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _isLoading = true;
        _settings = await ApiService.WorkspaceIntegrationSettingsGetAsync();
        _isLoading = false;
    }
}
