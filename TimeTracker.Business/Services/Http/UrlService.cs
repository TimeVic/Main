using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TimeTracker.Business.Services.Http;

public class UrlService: IUrlService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _frontendUrl;

    public UrlService(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
    }

    public string ToFrontendAbsoluteUrl(string relativePath)
    {
        relativePath = string.IsNullOrWhiteSpace(relativePath)
            ? "/"
            : relativePath.StartsWith("/") ? relativePath : $"/{relativePath}";

        var frontendUrl = string.IsNullOrWhiteSpace(_frontendUrl)
            ? GetCurrentRequestBaseUrl()
            : _frontendUrl;

        return $"{frontendUrl}{relativePath}";
    }

    private string GetCurrentRequestBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            return string.Empty;
        }

        return $"{request.Scheme}://{request.Host}";
    }
}
