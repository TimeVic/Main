using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Validation;

namespace TimeTracker.Web.Pages.User.Registration;

public partial class Step1
{
    [Inject] 
    private IApiService _apiService { get; set; }
    
    [Inject] 
    private NavigationManager _navigationManager { get; set; }

    [Inject] 
    private IReCaptchaService _reCaptchaService { get; set; }
    
    private RegistrationStep1Request model = new();
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
            var isOk = await _apiService.RegistrationStep1Async(model);
            if (isOk)
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Registration email is sent"
                });
                model.Email = "";
            }
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
