using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Ui.Pages.Landing.User.Password;

public partial class ResetPasswordPage
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
    private bool _isSent = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _isLoading = false;
        _isSent = false;
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
            var isSuccess = await _apiService.ResetPasswordStep1(model);
            if (isSuccess)
            {
                _toastService.ShowInfo(Localizer["Toast_EmailSent"]);
                model.Email = string.Empty;
                _isSent = true;
            }
            else
            {
                ToastService.ShowError(Localizer["Toast_IncorrectEmailOrPassword"]);
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
}
