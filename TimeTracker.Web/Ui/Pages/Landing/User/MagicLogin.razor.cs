using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Ui.Pages.Landing.User;

public partial class MagicLogin
{
    [Parameter]
    public string Token { get; set; }

    [Inject]
    private ApiService _apiService { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }

    [Inject]
    private IAuthorizationService _authorizationService { get; set; }

    private bool _isLoading = true;
    private bool _isError;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _isError = false;
        try
        {
            var loginResponse = await _apiService.VerifyMagicTokenAsync(new VerifyMagicTokenRequest
            {
                Token = Token
            });
            if (loginResponse == null)
            {
                _isError = true;
                return;
            }
            _authorizationService.Login(loginResponse.AccessToken, loginResponse.JwtToken, loginResponse.User);
            _navigationManager.NavigateTo(SiteUrl.DashboardBase);
        }
        catch (Exception)
        {
            _isError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
