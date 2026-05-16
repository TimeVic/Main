using System.Net;
using Microsoft.Net.Http.Headers;

namespace TimeTracker.Business.Testing.Extensions;

public static class HttpResponseMessageExtensionsExtensions
{
    public static string? GetSetCookieValue(this HttpResponseMessage response, string key)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            return null;
        }
        return setCookieValues
            .Select(cookieValue => SetCookieHeaderValue.Parse(cookieValue))
            .Where(item => item.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
            .Where(item => item.Value != null)
            .Select(item => WebUtility.UrlDecode(item.Value.Value))
            .FirstOrDefault();
    }
}
