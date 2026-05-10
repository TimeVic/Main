namespace TimeTracker.Web.Services;

public interface ISeoUrlService
{
    SeoMetadata GetPublicPageMetadata(string englishPath, string currentPath);
}
