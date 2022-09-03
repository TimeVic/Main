using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Business.Services.Http;

public class RequestService: IRequestService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IJwtAuthService _jwtAuthService;

    public RequestService(
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

    public long GetUserIdFromJwt()
    {
        return _jwtAuthService.GetUserId(GetApiToken());
    }
}
