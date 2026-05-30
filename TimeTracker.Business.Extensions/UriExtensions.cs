using System.Web;

namespace TimeTracker.Business.Extensions;

public static class UriExtensions
{
    public static Uri AddOrUpdateQueryParam(this Uri uri, string key, string? value)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[key] = Uri.EscapeDataString(value ?? string.Empty);
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }
}
