using System.Text.RegularExpressions;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class LoginBlock
{
    private string _login = string.Empty;
    private bool _isSaving;
    private string? _errorMessage;

    protected override Task OnInitializedAsync()
    {
        _login = AuthState.Value.User?.Login ?? string.Empty;
        return base.OnInitializedAsync();
    }

    private async Task OnChangeLogin()
    {
        var cleanLogin = _login.Trim().TrimStart('@').ToLower();
        if (string.IsNullOrWhiteSpace(cleanLogin))
        {
            _errorMessage = DashboardLocalizer["UserSettings_LoginRequired"].Value;
            return;
        }

        if (!Regex.IsMatch(cleanLogin, @"^[a-z0-9_]{3,60}$"))
        {
            _errorMessage = DashboardLocalizer["UserSettings_LoginInvalidFormat"].Value;
            return;
        }

        if (string.Equals(cleanLogin, AuthState.Value.User?.Login, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var updatedUser = await ApiService.UserChangeLoginAsync(cleanLogin);
            if (updatedUser != null)
            {
                _login = updatedUser.Login ?? cleanLogin;
                Dispatcher.Dispatch(new UpdateUserAction(updatedUser));
                ToastService.ShowSuccess(DashboardLocalizer["UserSettings_LoginSaved"].Value);
            }
        }
        catch (Exception)
        {
            _errorMessage = DashboardLocalizer["UserSettings_LoginChangeError"].Value;
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
