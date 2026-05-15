using System.Globalization;
using Microsoft.JSInterop;
using TimeTracker.Web.Services;

namespace TimeTracker.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class ProfileBlock
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _selectedLanguage = string.Empty;

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
}
