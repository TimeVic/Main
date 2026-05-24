using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services;

namespace TimeTracker.Web.Services;

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
            var storedLocale = await _js.InvokeAsync<string?>("localStorage.getItem", ILocalizationUrlService.LocalStorageKey);
            if (!string.Equals(NormalizeCultureName(storedLocale), userLocale, StringComparison.OrdinalIgnoreCase))
            {
                await _js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, userLocale);
            }

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

    private static string NormalizeCultureName(string? cultureName)
    {
        return cultureName == ILocalizationUrlService.UkrainianCultureName
            ? ILocalizationUrlService.UkrainianCultureName
            : ILocalizationUrlService.EnglishCultureName;
    }
}
