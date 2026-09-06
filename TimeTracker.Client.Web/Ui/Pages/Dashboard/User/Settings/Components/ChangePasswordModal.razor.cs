using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class ChangePasswordModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    private bool _isSaving;
    private string? _errorMessage;
    private readonly ChangePasswordModel _model = new();

    private async Task CloseModal()
    {
        _errorMessage = null;
        _model.Clear();
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Cancel());
        }
    }

    private async Task OnChangePassword()
    {
        if (string.IsNullOrWhiteSpace(_model.CurrentPassword)
            || string.IsNullOrWhiteSpace(_model.NewPassword)
            || _model.NewPassword != _model.ConfirmPassword)
        {
            _errorMessage = DashboardLocalizer["UserSettings_PasswordMismatch"].Value;
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        try
        {
            await ApiService.UserChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword = _model.CurrentPassword,
                NewPassword = _model.NewPassword
            });
            ToastService.ShowSuccess(DashboardLocalizer["UserSettings_PasswordChanged"].Value);
            if (ModalInstance != null)
            {
                await ModalInstance.Close(AppModalResult.Ok());
            }
        }
        catch (Exception)
        {
            _errorMessage = DashboardLocalizer["UserSettings_PasswordChangeError"].Value;
        }
        finally
        {
            _isSaving = false;
        }
    }

    public sealed class ChangePasswordModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public void Clear() { CurrentPassword = NewPassword = ConfirmPassword = string.Empty; }
    }
}
