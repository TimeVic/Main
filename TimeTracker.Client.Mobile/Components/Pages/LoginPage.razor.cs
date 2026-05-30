using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Client.Core.Services;

namespace TimeTracker.Client.Mobile.Components.Pages;

public partial class LoginPage
{
    private const string BoardPath = "/board";
    private const string MobileReCaptchaToken = "mobile";

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = null!;

    private readonly LoginRequest _model = new()
    {
        ReCaptcha = MobileReCaptchaToken,
        IsRememberMe = true
    };

    private EditForm? _form;
    private bool _isLoading;
    private string? _errorMessage;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (AuthState.Value.IsLoggedIn)
        {
            NavigationManager.NavigateTo(BoardPath, replace: true);
        }
    }

    private async Task SubmitAsync()
    {
        _errorMessage = null;
        _model.ReCaptcha = MobileReCaptchaToken;

        if (_form?.EditContext?.Validate() != true)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var isLoggedIn = await AuthorizationService.LoginAsync(_model);
            if (!isLoggedIn)
            {
                _errorMessage = Localizer["Error_InvalidCredentials"];
                return;
            }

            NavigationManager.NavigateTo(BoardPath, replace: true);
        }
        catch (Exception)
        {
            _errorMessage = Localizer["Error_InvalidCredentials"];
        }
        finally
        {
            _isLoading = false;
        }
    }
}
