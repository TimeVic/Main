using System.Globalization;
using TimeTracker.Web.Constants;

namespace TimeTracker.Web.Services;

public class LocalizationUrlService : ILocalizationUrlService
{
    private static readonly string UkrainianPrefix = SiteUrl.UkLocalePrefix;

    public bool IsUkrainianPath(string path)
        => path == UkrainianPrefix || path.StartsWith(UkrainianPrefix + "/");

    public string GetCurrentCultureName(string path)
        => IsUkrainianPath(path) ? ILocalizationUrlService.UkrainianCultureName : ILocalizationUrlService.EnglishCultureName;

    public void ApplyCultureFromPath(string path)
        => ApplyCulture(GetCurrentCultureName(path));

    public void ApplyCulture(string cultureName)
    {
        var normalizedCultureName = cultureName == ILocalizationUrlService.UkrainianCultureName
            ? ILocalizationUrlService.UkrainianCultureName
            : ILocalizationUrlService.EnglishCultureName;
        var culture = new CultureInfo(normalizedCultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public string GetEnglishUrl(string path)
    {
        if (!IsUkrainianPath(path))
            return path;

        var stripped = path[UkrainianPrefix.Length..];
        return string.IsNullOrEmpty(stripped) ? "/" : stripped;
    }

    public string GetUkrainianUrl(string path)
    {
        if (IsUkrainianPath(path))
            return path;

        return path == "/" ? UkrainianPrefix + "/" : UkrainianPrefix + path;
    }

    public string GetLocalizedUrl(string path, string targetCulture)
        => targetCulture == ILocalizationUrlService.UkrainianCultureName ? GetUkrainianUrl(path) : GetEnglishUrl(path);
}
