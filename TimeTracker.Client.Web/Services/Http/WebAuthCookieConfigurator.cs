using Microsoft.AspNetCore.Components.WebAssembly.Http;
using TimeTracker.Client.Core.Services.Http.Cookies;

namespace TimeTracker.Client.Web.Services.Http;

public class WebAuthCookieConfigurator : IAuthCookieConfigurator
{
    public Task ConfigureRequestAsync(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return Task.CompletedTask;
    }

    public Task ProcessResponseAsync(HttpResponseMessage response)
    {
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        return Task.CompletedTask;
    }
}
