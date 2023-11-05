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
        var absPath = e.Request.RequestUri.AbsolutePath;
        Debug.Log("RefreshAuthTokenAsync", absPath, ExcludedUrls);
        var isExcludedUrl = ExcludedUrls.Any(excludedUrl => absPath.StartsWith(excludedUrl));
        Debug.Log("isExcludedUrl", isExcludedUrl);
        if (!isExcludedUrl)
        {
            var jwtToken = await _refreshJwtTokenService.TryRefreshToken();
            if(!string.IsNullOrEmpty(jwtToken))
            {
                e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
        else
        {
            Debug.Log("Excluded URL:", absPath);
        }
    }
}
