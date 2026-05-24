using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Web.Services;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Web.Ui.Pages.Landing.User;

public partial class LoginAsDemoPage
{
    [Inject]
    private ApiService _apiService { get; set; } = default!;

    [Inject]
    private NavigationManager _navigationManager { get; set; } = default!;

    [Inject]
    private IAuthorizationService _authorizationService { get; set; } = default!;

    private bool _isError;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await base.OnAfterRenderAsync(firstRender);
        await LoadDemoAsync();
    }

    private async Task LoadDemoAsync()
    {
        try
        {
            var response = await _apiService.LoginAsDemoAsync();
            if (response == null || response.User.Id == Guid.Empty)
            {
                throw new Exception("Empty demo response");
            }
            _authorizationService.Login(response.User);
            _navigationManager.NavigateTo(SiteUrl.DashboardBase);
        }
        catch (Exception)
        {
            _isError = true;
            StateHasChanged();
        }
    }
}
