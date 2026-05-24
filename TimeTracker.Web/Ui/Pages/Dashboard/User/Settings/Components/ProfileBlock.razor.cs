using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class ProfileBlock
{
    [Inject]
    public UrlService UrlService { get; set; } = null!;

    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _selectedLanguage = string.Empty;
    private bool _isSaving;
    private bool _isUploadingAvatar;
    private bool _isDeletingAvatar;
    private string _avatarRenderKey => AuthState.Value.User?.Avatar?.Id.ToString() ?? "avatar-empty";
    private string? _avatarSrc => AuthState.Value.User?.Avatar == null
        ? null
        : UrlService.GetStorageImageUrl(AuthState.Value.User.Avatar, StorageImageSize.S_256);

    protected override Task OnInitializedAsync()
    {
        var user = AuthState.Value.User;
        _name = user?.UserName ?? string.Empty;
        _email = user?.Email ?? string.Empty;
        _selectedLanguage = user?.Language?.Code
            ?? (CultureInfo.CurrentUICulture.Name == ILocalizationUrlService.UkrainianCultureName
                ? ILocalizationUrlService.UkrainianCultureName
                : ILocalizationUrlService.EnglishCultureName);
        return base.OnInitializedAsync();
    }

    private async Task OnSave()
    {
        _isSaving = true;
        StateHasChanged();

        try
        {
            var user = await ApiService.UserUpdateSettingsAsync(new UpdateSettingsRequest
            {
                UserName = _name,
                LanguageCode = _selectedLanguage
            });
            if (user == null)
            {
                return;
            }

            Dispatcher.Dispatch(new UpdateUserAction(user));
            await Js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, user.Language?.Code ?? _selectedLanguage);

            var currentCulture = CultureInfo.CurrentUICulture.Name == ILocalizationUrlService.UkrainianCultureName
                ? ILocalizationUrlService.UkrainianCultureName
                : ILocalizationUrlService.EnglishCultureName;
            if (!string.Equals(currentCulture, user.Language?.Code, StringComparison.OrdinalIgnoreCase))
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(DashboardLocalizer["UserSettings_SaveError"].Value);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
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
            // Delete the existing avatar before uploading a new one.
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
                UpdateCurrentUserAvatar(avatarDto);
                Dispatcher.Dispatch(new LoadCurrentUserAction());
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(DashboardLocalizer["UserSettings_AvatarUploadError"].Value);
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
            UpdateCurrentUserAvatar(null);
            Dispatcher.Dispatch(new LoadCurrentUserAction());
        }
        catch (Exception)
        {
            ToastService.ShowError(DashboardLocalizer["UserSettings_AvatarDeleteError"].Value);
        }
        finally
        {
            _isDeletingAvatar = false;
            StateHasChanged();
        }
    }

    private void UpdateCurrentUserAvatar(StoredFileDto? avatar)
    {
        var user = AuthState.Value.User;
        if (user == null)
        {
            return;
        }

        // Keep the profile and header avatars in sync immediately after upload/delete.
        Dispatcher.Dispatch(new UpdateUserAction(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Timezone = user.Timezone,
            DefaultWorkspace = user.DefaultWorkspace,
            SelectedWorkspace = user.SelectedWorkspace,
            Language = user.Language,
            Avatar = avatar
        }));
    }
}
