using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Ui.Pages.Landing.User;

public partial class LoginPage
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
    private LoginMagicRequest _magicModel = new();

    private bool _isLoading;
    private bool _magicIsLoading;
    private bool _magicIsSent;
    private EditForm _form;
    private EditForm _magicForm;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _isLoading = false;
        await UpdateReCaptchaAsync();
    }

    private async Task UpdateReCaptchaAsync()
    {
        var token = await _reCaptchaService.GetReCaptchaTokenAsync();
        model.ReCaptcha = token;
        _magicModel.ReCaptcha = token;
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
            var loginResponse = await _apiService.LoginAsync(model);
            if (loginResponse == null)
            {
                throw new Exception("Login error");
            }
            if (!string.IsNullOrEmpty(loginResponse.JwtToken))
            {
                _authorizationService.Login(loginResponse.AccessToken, loginResponse.JwtToken, loginResponse.User);
                _navigationManager.NavigateTo(SiteUrl.DashboardBase);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(Localizer["Toast_IncorrectEmailOrPassword"]);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }

    private void ForgotPassword()
    {
        NavigationManager.NavigateTo(LocalizationUrlService.GetLocalizedUrl(SiteUrl.ForgotPassword, CurrentCulture));
    }

    private async Task SubmitMagic()
    {
        if (!_magicForm.EditContext!.Validate())
        {
            return;
        }
        _magicIsLoading = true;
        _magicIsSent = false;
        try
        {
            var isOk = await _apiService.LoginMagicAsync(_magicModel);
            if (isOk)
            {
                _magicIsSent = true;
                _magicModel.Email = string.Empty;
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(Localizer["Toast_FailedToSendMagicLink"]);
        }
        finally
        {
            _magicIsLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
