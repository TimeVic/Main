namespace TimeTracker.Client.Core.Services.Http.Cookies;

public interface IAuthCookieConfigurator
{
    Task ConfigureRequestAsync(HttpRequestMessage request);

    Task ProcessResponseAsync(HttpResponseMessage response);

    Task ClearAsync();
}
