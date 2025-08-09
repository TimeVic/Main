using System.Net.Http.Headers;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http.Auth;
using Toolbelt.Blazor;

namespace TimeTracker.Web.Services.Http.Middleware;

public class HttpInterceptorService
{
    private readonly HttpClientInterceptor _interceptor;
    private readonly IConfiguration _configuration;
    private readonly RefreshJwtTokenService _refreshJwtTokenService;
    private readonly ILogger<HttpInterceptorService> _logger;

    private string[] ExcludedUrls => _configuration.GetSection("Auth:ExcludedApiUrls").Get<string[]>() ?? [];

    public HttpInterceptorService(
        HttpClientInterceptor interceptor,
        IConfiguration configuration,
        RefreshJwtTokenService refreshJwtTokenService,
        ILogger<HttpInterceptorService> logger
    )
    {
        _interceptor = interceptor;
        _configuration = configuration;
        _refreshJwtTokenService = refreshJwtTokenService;
        _logger = logger;
    }

    public void Register() => _interceptor.BeforeSendAsync += RefreshAuthTokenAsync;
    
    public void Unregister() => _interceptor.BeforeSendAsync -= RefreshAuthTokenAsync;

    public async Task RefreshAuthTokenAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        var absPath = e.Request.RequestUri!.AbsolutePath;
        var isExcludedUrl = ExcludedUrls.Any(excludedUrl => absPath.StartsWith(excludedUrl));
        if (!isExcludedUrl)
        {
            _logger.LogInformation($"Intercept API request and try to receive JWT token: {absPath}");
            var jwtToken = await _refreshJwtTokenService.TryRefreshToken();
            if(!string.IsNullOrEmpty(jwtToken))
            {
                e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
    }
}
