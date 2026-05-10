namespace TimeTracker.Web.Services;

/// <summary>
/// Handles URL-based locale detection and localized URL generation for public pages.
/// Culture is determined from the URL path: /uk prefix = uk-UA, otherwise = en.
/// </summary>
public interface ILocalizationUrlService
{
    static string UkrainianCultureName => "uk-UA";
    static string EnglishCultureName => "en";

    bool IsUkrainianPath(string path);
    string GetCurrentCultureName(string path);

    /// <summary>Applies DefaultThreadCurrentCulture/UICulture based on the provided URL path.</summary>
    void ApplyCultureFromPath(string path);

    void ApplyCulture(string cultureName);

    /// <summary>Returns the English (default) version of the given path by stripping the /uk prefix.</summary>
    string GetEnglishUrl(string path);

    /// <summary>Returns the Ukrainian version of the given path by adding the /uk prefix.</summary>
    string GetUkrainianUrl(string path);

    /// <summary>Returns the given path localized to the target culture.</summary>
    string GetLocalizedUrl(string path, string targetCulture);
}
