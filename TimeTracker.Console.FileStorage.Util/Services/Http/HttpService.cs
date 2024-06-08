using TimeTracker.Business.FileStorage.Commons.Constants;

namespace TimeTracker.Console.FileStorage.Util.Services.Http;

public class HttpService: IHttpService
{
    private HttpClient CreateClient(bool isAuthorized = true)
    {
        var httpClient = new HttpClient();
        if (isAuthorized)
        {
            httpClient.DefaultRequestHeaders.Add(HttpHeader.ApiKey, "");
            httpClient.DefaultRequestHeaders.Add(HttpHeader.ApiSecret, "");
        }
        return httpClient;
    }

    public async Task<bool> IsValidCredentials()
    {
        var httpClient = CreateClient(false);
        return false;
    }
}
