using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Ui.Pages.Landing.User.Password;

public partial class ChangePassword
{
    [Parameter] 
    public string Token { get; set; }
    
    [Inject] 
    private ApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    [Inject] 
    private IAuthorizationService _authorizationService { get; set; }
    
    [Inject] 
    private ToastService _toastService { get; set; }
    
    private ResetPasswordStep2Request model = new();
    private bool _isLoading;
    private EditForm _form;
    private bool _isValid = false;
    private bool _isCompleted = false;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = false;
        _isCompleted = false;
        model.VerficationToken = Token;
        await UpdateReCaptchaAsync();
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
            var isSuccess = await _apiService.ResetPasswordStep2(model);
            if (isSuccess)
            {
                _isCompleted = true;
                _toastService.ShowInfo("Your password has been changed");
                NavigationManager.NavigateTo("/");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("Incorrect email or password");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
