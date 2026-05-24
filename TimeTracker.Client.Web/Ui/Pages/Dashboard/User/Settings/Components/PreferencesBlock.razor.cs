using System.Globalization;
using Microsoft.JSInterop;
using TimeTracker.Client.Web.Services;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class PreferencesBlock
{
    private string _selectedLanguage = string.Empty;

    protected override void OnInitialized()
    {
        _selectedLanguage = CultureInfo.CurrentUICulture.Name == ILocalizationUrlService.UkrainianCultureName
            ? ILocalizationUrlService.UkrainianCultureName
            : ILocalizationUrlService.EnglishCultureName;
        base.OnInitialized();
    }

    private async Task OnSave()
    {
        await Js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, _selectedLanguage);
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }
}
