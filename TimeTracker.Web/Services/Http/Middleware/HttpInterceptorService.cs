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

    private readonly object _requestLockObject = new();
    
    private string[] ExcludedUrls
    {
        get
        {
            return _configuration.GetSection("Auth:ExcludedApiUrls")
                .Get<string[]>() ?? Array.Empty<string>();
        }
    }

    public HttpInterceptorService(
        HttpClientInterceptor interceptor,
        IConfiguration configuration,
        RefreshJwtTokenService refreshJwtTokenService
    )
    {
        _interceptor = interceptor;
        _configuration = configuration;
        _refreshJwtTokenService = refreshJwtTokenService;
    }

    public void Register() => _interceptor.BeforeSendAsync += RefreshAuthTokenAsync;
    
    public void Unregister() => _interceptor.BeforeSendAsync -= RefreshAuthTokenAsync;

    public async Task RefreshAuthTokenAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        lock (_requestLockObject)
        {
            var absPath = e.Request.RequestUri!.AbsolutePath;
            var isExcludedUrl = ExcludedUrls.Any(excludedUrl => absPath.StartsWith(excludedUrl));
            if (!isExcludedUrl)
            {
                var jwtToken = _refreshJwtTokenService.TryRefreshToken().Result;
                if(!string.IsNullOrEmpty(jwtToken))
                {
                    e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
            }    
        }
    }
}
