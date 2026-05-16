using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Mvc.Middleware;

public class JwtRefreshMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<JwtRefreshMiddleware> _logger;

    private readonly TimeSpan _jwtExpirationDelay = TimeSpan.FromMinutes(30);
    
    public JwtRefreshMiddleware(
        RequestDelegate next,
        ILogger<JwtRefreshMiddleware> logger,
        IConfiguration configuration
    )
    {
        _next = next;
        _logger = logger;
        _jwtExpirationDelay = TimeSpan.FromMinutes(configuration.GetValue("App:Auth:JwtRefreshDelay", 30));
    }

    public async Task InvokeAsync(HttpContext context, ILifetimeScope scope)
    {
        try
        {
            var jwtAuthService = scope.Resolve<IJwtAuthService>();
            var authorizationService = scope.Resolve<IAuthorizationService>();
            var apiRequestService = scope.Resolve<IApiRequestService>();

            var jwt = apiRequestService.GetApiToken();
            if (!string.IsNullOrEmpty(jwt) && jwtAuthService.IsJwt(jwt))
            {
                var accessToken = apiRequestService.GetAccessToken();
                
                if (!string.IsNullOrEmpty(accessToken) && jwtAuthService.IsValidJwt(jwt, false))
                {
                    if (jwtAuthService.IsTokenExpired(jwt, _jwtExpirationDelay))
                    {
                        _logger.LogDebug("Refresh JWT token...");
                        var loginResult = await authorizationService.GenerateNewJwtToken(accessToken, jwt);
                        var cookiesService = scope.Resolve<IHttpCookiesService>();
                        var httpHeadersService = scope.Resolve<IHttpHeadersService>();
                
                        _logger.LogDebug($"Cookie with JWT token was refreshed. Expiration time: {jwtAuthService.GetTokenExpirationTime(loginResult.JwtToken)}");
                        context.Request.Headers.Authorization = $"Bearer {loginResult.JwtToken}";
                
                        cookiesService.Append(
                            context,
                            HttpCookieKeyEnum.JwtToken, 
                            loginResult.JwtToken, 
                            DateTimeOffset.UtcNow.AddDays(30)
                        );
                        httpHeadersService.Append(HttpHeaderKeyEnum.JwtToken, loginResult.JwtToken);
                    }
                }
            }
        }
        catch (InvalidTokenException e)
        {
            _logger.LogTrace(e.Message);
        }
        catch (IncorrectAccessTokenException e)
        {
            _logger.LogTrace(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        finally
        {       
            await _next(context);
        }
    }
}
