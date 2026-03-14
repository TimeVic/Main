using System.Net;
using System.Net.Http.Headers;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.Http.Auth;
using TimeTracker.Web.Store.Auth;
using Toolbelt.Blazor;

namespace TimeTracker.Web.Services.Http.Middleware;

public class HttpInterceptorService
{
    private readonly HttpClientInterceptor _interceptor;
    private readonly IConfiguration _configuration;
    private readonly RefreshJwtTokenService _refreshJwtTokenService;
    private readonly ILogger<HttpInterceptorService> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly IDispatcher _dispatcher;

    private string[] ExcludedUrls => _configuration.GetSection("Auth:ExcludedApiUrls").Get<string[]>() ?? [];

    public HttpInterceptorService(
        HttpClientInterceptor interceptor,
        IConfiguration configuration,
        RefreshJwtTokenService refreshJwtTokenService,
        ILogger<HttpInterceptorService> logger,
        NavigationManager navigationManager,
        IDispatcher dispatcher
    )
    {
        _interceptor = interceptor;
        _configuration = configuration;
        _refreshJwtTokenService = refreshJwtTokenService;
        _logger = logger;
        _navigationManager = navigationManager;
        _dispatcher = dispatcher;
    }

    public void Register()
    {
        _interceptor.BeforeSendAsync += RefreshAuthTokenAsync;
        _interceptor.AfterSendAsync += CheckResponseAsync;
    }

    public void Unregister()
    {
        _interceptor.BeforeSendAsync -= RefreshAuthTokenAsync;
        _interceptor.AfterSendAsync -= CheckResponseAsync;
    }

    private async Task RefreshAuthTokenAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        var absPath = e.Request.RequestUri!.AbsolutePath;
        var isExcludedUrl = ExcludedUrls.Any(excludedUrl => absPath.StartsWith(excludedUrl));
        if (!isExcludedUrl)
        {
            var jwtToken = await _refreshJwtTokenService.TryRefreshToken();
            if(!string.IsNullOrEmpty(jwtToken))
            {
                e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
    }
    
    private async Task CheckResponseAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        if (e.Response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var accessToken = await _refreshJwtTokenService.GetAccessToken();
            if (accessToken != null)
            {
                _dispatcher.Dispatch(new LogoutAction());
            }
            if (!_navigationManager.Uri.EndsWith(SiteUrl.Login))
            {
                _navigationManager.NavigateTo(SiteUrl.Login);
            }
        }
    }
}
