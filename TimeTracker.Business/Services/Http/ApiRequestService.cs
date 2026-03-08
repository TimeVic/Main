using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Business.Services.Http;

public class ApiRequestService: IApiRequestService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IJwtAuthService _jwtAuthService;

    public ApiRequestService(
        IHttpContextAccessor httpContext,
        IJwtAuthService jwtAuthService
    )
    {
        _httpContext = httpContext;
        _jwtAuthService = jwtAuthService;
    }

    public string GetApiToken()
    {
        return _httpContext.HttpContext?.Request.GetApiToken();
    }

    public Guid GetUserIdFromJwt()
    {
        return _jwtAuthService.GetUserId(GetApiToken());
    }
    
    public string? GetRequestUrl()
    {
        return _httpContext?.HttpContext?.Request.Path.Value?.ToLower();
    }
}
