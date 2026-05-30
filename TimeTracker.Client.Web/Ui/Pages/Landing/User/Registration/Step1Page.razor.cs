using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Web.Services.Validation;

namespace TimeTracker.Client.Web.Ui.Pages.Landing.User.Registration;

public partial class Step1Page
{
    [Inject] 
    private ApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }

    [Inject]
    private ILocalizationUrlService LocalizationUrlService { get; set; }
    
    private RegistrationStep1Request model = new();
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
            model.LanguageCode = LocalizationUrlService.GetCurrentCultureName(new Uri(NavigationManager.Uri).AbsolutePath);
            var isOk = await _apiService.RegistrationStep1Async(model);
            if (isOk)
            {
                _isSent = true;
                ToastService.ShowInfo(Localizer["Toast_RegistrationEmailSent"]);
                model.Email = string.Empty;
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(Localizer["Toast_RegistrationError"]);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
