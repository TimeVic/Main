using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Pages.Landing.User.Registration;

public partial class Step2
{
    [Parameter]
    public string? VerificationToken
    {
        get => model.Token;
        set => model.Token = value;
    }
    
    [Inject] 
    private IApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IAuthorizationService _authorizationService { get; set; }
    
    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    private RegistrationStep2Request model = new();
    private EditContext _editContext;
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
            var registrationResponse = await _apiService.RegistrationStep2Async(model);
            _authorizationService.Login(registrationResponse.JwtToken);
            _navigationManager.NavigateTo(SiteUrl.DashboardBase);
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Registration error"
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
