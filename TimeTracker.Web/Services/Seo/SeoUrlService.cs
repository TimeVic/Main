using TimeTracker.Client.Core.Constants;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Services;

public class SeoUrlService : ISeoUrlService
{
    private readonly ILocalizationUrlService _localizationUrlService;
    private readonly UrlService _urlService;

    public SeoUrlService(
        ILocalizationUrlService localizationUrlService,
        UrlService urlService
    )
    {
        _localizationUrlService = localizationUrlService;
        _urlService = urlService;
    }

    public SeoMetadata GetPublicPageMetadata(string englishPath, string currentPath)
    {
        var normalizedEnglishPath = NormalizeEnglishPath(englishPath);
        var normalizedCurrentPath = NormalizePath(currentPath);
        var isUkrainianPage = _localizationUrlService.IsUkrainianPath(normalizedCurrentPath);
        var canonicalPath = isUkrainianPage
            ? _localizationUrlService.GetUkrainianUrl(normalizedEnglishPath)
            : normalizedEnglishPath;

        var englishUrl = _urlService.ToAbsoluteUrl(normalizedEnglishPath);
        var ukrainianUrl = _urlService.ToAbsoluteUrl(_localizationUrlService.GetUkrainianUrl(normalizedEnglishPath));

        return new SeoMetadata(
            _urlService.ToAbsoluteUrl(canonicalPath),
            isUkrainianPage ? ILocalizationUrlService.UkrainianCultureName : ILocalizationUrlService.EnglishCultureName,
            [
                new SeoAlternateUrl("en", englishUrl),
                new SeoAlternateUrl("uk-UA", ukrainianUrl),
                new SeoAlternateUrl("x-default", englishUrl),
            ]
        );
    }

    private static string NormalizeEnglishPath(string path)
    {
        var normalizedPath = NormalizePath(path);

        if (normalizedPath == SiteUrl.UkLocalePrefix || normalizedPath.StartsWith(SiteUrl.UkLocalePrefix + "/"))
        {
            normalizedPath = normalizedPath[SiteUrl.UkLocalePrefix.Length..];
        }

        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return SiteUrl.Main;
        }

        return normalizedPath == "/" ? SiteUrl.Main : normalizedPath.TrimEnd('/');
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return SiteUrl.Main;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            path = uri.AbsolutePath;
        }

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return path == "/" ? SiteUrl.Main : path.TrimEnd('/');
    }
}
