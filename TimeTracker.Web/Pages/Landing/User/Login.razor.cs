using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Pages.Landing.User;

public partial class Login
{
    [Inject] 
    private ApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    [Inject] 
    private IAuthorizationService _authorizationService { get; set; }
    
    private LoginRequest model = new();
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = false;
        await UpdateReCaptchaAsync();
    }

    private async Task UpdateReCaptchaAsync()
    {
        model.ReCaptcha = await _reCaptchaService.GetReCaptchaTokenAsync();
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var loginResponse = await _apiService.LoginAsync(model);
            if (!string.IsNullOrEmpty(loginResponse.Token))
            {
                _authorizationService.Login(loginResponse.Token, loginResponse.User);
                _navigationManager.NavigateTo(SiteUrl.DashboardBase);
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Incorrect email or password"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
