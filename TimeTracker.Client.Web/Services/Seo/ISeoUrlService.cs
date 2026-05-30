namespace TimeTracker.Client.Web.Services;

public interface ISeoUrlService
{
    SeoMetadata GetPublicPageMetadata(string englishPath, string currentPath);
}
