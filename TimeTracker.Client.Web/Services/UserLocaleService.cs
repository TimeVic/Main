using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Client.Core.Services;

namespace TimeTracker.Client.Web.Services;

public class UserLocaleService : IUserLocaleService
{
    private readonly IJSRuntime _js;
    private readonly ILocalizationUrlService _localizationUrlService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<UserLocaleService> _logger;

    public UserLocaleService(
        IJSRuntime js,
        ILocalizationUrlService localizationUrlService,
        NavigationManager navigationManager,
        ILogger<UserLocaleService> logger
    )
    {
        _js = js;
        _localizationUrlService = localizationUrlService;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public async Task ApplyUserLocaleAsync(UserDto user)
    {
        var userLocale = NormalizeCultureName(user.Language?.Code);
        if (string.IsNullOrEmpty(userLocale))
        {
            return;
        }

        try
        {
            var currentPath = new Uri(_navigationManager.Uri).AbsolutePath;
            if (_localizationUrlService.IsUkrainianPath(currentPath))
            {
                // Prevent reload loops when a public localized URL conflicts with the saved user language.
                await StoreLocaleAsync(ILocalizationUrlService.UkrainianCultureName);
                _localizationUrlService.ApplyCulture(ILocalizationUrlService.UkrainianCultureName);
                return;
            }

            await StoreLocaleAsync(userLocale);

            var currentLocale = NormalizeCultureName(CultureInfo.CurrentUICulture.Name);
            if (string.Equals(currentLocale, userLocale, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _localizationUrlService.ApplyCulture(userLocale);
            _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private async Task StoreLocaleAsync(string locale)
    {
        try
        {
            var storedLocale = await _js.InvokeAsync<string?>("localStorage.getItem", ILocalizationUrlService.LocalStorageKey);
            if (!string.Equals(NormalizeCultureName(storedLocale), locale, StringComparison.OrdinalIgnoreCase))
            {
                await _js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, locale);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private static string NormalizeCultureName(string? cultureName)
    {
        return CultureCodeHelper.GetSupportedCultureCode(cultureName)
            ?? CultureCodeHelper.EnglishCultureCode;
    }
}
