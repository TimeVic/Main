namespace TimeTracker.Web.Services;

public sealed record SeoMetadata(
    string CanonicalUrl,
    string DocumentLanguage,
    IReadOnlyCollection<SeoAlternateUrl> AlternateUrls
);
