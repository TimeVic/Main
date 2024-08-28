using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Pages.Landing.User.Password;

public partial class ResetPassword
{
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
    
    private ResetPasswordStep1Request model = new();
    private bool _isLoading;
    private EditForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = false;
        await UpdateReCaptchaAsync();
    }

    private async Task UpdateReCaptchaAsync()
    {
        model.ReCaptcha = await _reCaptchaService.GetReCaptchaTokenAsync();
    }
    
    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        _isLoading = true;
        try
        {
            var isSuccess = await _apiService.ResetPasswordStep1(model);
            if (isSuccess)
            {
                await _toastService.ShowInfo("Email has been sent");
                model.Email = string.Empty;
            }
        }
        catch (Exception)
        {
            await ToastService.ShowError("Incorrect email or password");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
