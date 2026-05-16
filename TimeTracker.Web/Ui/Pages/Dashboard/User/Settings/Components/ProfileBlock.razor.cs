using System.Globalization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TimeTracker.Web.Services;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class ProfileBlock
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _selectedLanguage = string.Empty;
    private bool _isUploadingAvatar;
    private bool _isDeletingAvatar;

    protected override Task OnInitializedAsync()
    {
        var user = AuthState.Value.User;
        _name = user?.UserName ?? string.Empty;
        _email = user?.Email ?? string.Empty;
        _selectedLanguage = CultureInfo.CurrentUICulture.Name == ILocalizationUrlService.UkrainianCultureName
            ? ILocalizationUrlService.UkrainianCultureName
            : ILocalizationUrlService.EnglishCultureName;
        return base.OnInitializedAsync();
    }

    private async Task OnSave()
    {
        // TODO: Wire to user profile update API
        await Js.InvokeVoidAsync("localStorage.setItem", "timevic.locale", _selectedLanguage);
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }

    private async Task OnClickChangeAvatar()
    {
        await Js.InvokeVoidAsync("eval", "document.getElementById('avatar-file-input').click()");
    }

    private async Task OnAvatarFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null)
        {
            return;
        }

        var userId = AuthState.Value.User?.Id;
        if (userId == null)
        {
            return;
        }

        _isUploadingAvatar = true;
        StateHasChanged();

        try
        {
            // Delete the existing avatar before uploading a new one
            var existingAvatarId = AuthState.Value.User?.Avatar?.Id;
            if (existingAvatarId.HasValue)
            {
                await ApiService.StorageDeleteFileAsync(existingAvatarId.Value);
            }

            var avatarDto = await ApiService.StorageUploadFileAsync(
                userId.Value,
                TimeTracker.Business.Common.Constants.Storage.StorageEntityType.User,
                TimeTracker.Business.Common.Constants.Storage.StoredFileType.Avatar,
                file
            );
            if (avatarDto != null)
            {
                await RefreshCurrentUserAsync();
            }
        }
        finally
        {
            _isUploadingAvatar = false;
            StateHasChanged();
        }
    }

    private async Task OnClickRemoveAvatar()
    {
        var avatarId = AuthState.Value.User?.Avatar?.Id;
        if (avatarId == null)
        {
            return;
        }

        _isDeletingAvatar = true;
        StateHasChanged();

        try
        {
            await ApiService.StorageDeleteFileAsync(avatarId.Value);
            await RefreshCurrentUserAsync();
        }
        finally
        {
            _isDeletingAvatar = false;
            StateHasChanged();
        }
    }

    private async Task RefreshCurrentUserAsync()
    {
        var user = await ApiService.UserGetCurrentAsync();
        if (user == null)
        {
            return;
        }

        // Refresh user data so avatar metadata comes from the committed profile state after upload/delete.
        Dispatcher.Dispatch(new UpdateUserAction(user));
        Dispatcher.Dispatch(new PersistDataAction());
    }
}
