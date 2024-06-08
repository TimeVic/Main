using System.Net.Http.Headers;
using System.Text;

namespace TimeTracker.Business.Extensions;

public static class HttpClientExtensions
{
    public static void AddBasicAuthCredentials(this HttpClient httpClient, string userName, string password)
    {
        var authenticationString = $"{userName}:{password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            base64EncodedAuthenticationString
        );
    }
}
