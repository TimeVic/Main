using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Ui.Pages.Landing.User.Registration;

public partial class Step2Page
{
    [Parameter]
    public string? VerificationToken { get; set; }
    
    [Inject] 
    private ApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IAuthorizationService _authorizationService { get; set; }
    
    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    private readonly RegistrationStep2Request model = new();
    private bool _isLoading;
    private EditForm _form = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _isLoading = false;
        await UpdateReCaptchaAsync();
    }

    protected override void OnParametersSet()
    {
        model.Token = VerificationToken ?? string.Empty;
    }

    private async Task UpdateReCaptchaAsync()
    {
        model.ReCaptcha = await _reCaptchaService.GetReCaptchaTokenAsync();
    }
    
    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        _isLoading = true;
        try
        {
            var registrationResponse = await _apiService.RegistrationStep2Async(model);
            if (registrationResponse == null)
                throw new Exception("Login error");
            _authorizationService.Login(
                registrationResponse.AccessToken,
                registrationResponse.JwtToken,
                registrationResponse.User
            );
            _navigationManager.NavigateTo(SiteUrl.DashboardBase);
        }
        catch (Exception)
        {
            ToastService.ShowError("Registration error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
