using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Pages.Landing.User.Registration;

public partial class Step1
{
    [Inject] 
    private ApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    private RegistrationStep1Request model = new();
    private bool _isLoading;
    private MudForm _form;
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
            var isOk = await _apiService.RegistrationStep1Async(model);
            if (isOk)
            {
                await ToastService.ShowInfo("Registration email is sent");
                model.Email = string.Empty;
            }
        }
        catch (Exception)
        {
            await ToastService.ShowError("Registration error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
        await UpdateReCaptchaAsync();
    }
}
