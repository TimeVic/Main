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

    private readonly TimeSpan _jwtExpirationDelay = TimeSpan.FromMinutes(5);
    
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
            var cookiesService = scope.Resolve<IHttpCookiesService>();

            var jwt = apiRequestService.GetApiToken()
                      ?? cookiesService.Get(context, HttpCookieKeyEnum.JwtToken.GetKey());
            if (!string.IsNullOrEmpty(jwt) && jwtAuthService.IsJwt(jwt))
            {
                var accessToken = apiRequestService.GetAccessToken()
                                  ?? cookiesService.Get(context, HttpCookieKeyEnum.AccessToken.GetKey());

                if (jwtAuthService.IsValidJwt(jwt, false))
                {
                    context.Request.Headers.Authorization = $"Bearer {jwt}";

                    if (jwtAuthService.IsTokenExpired(jwt, _jwtExpirationDelay))
                    {
                        _logger.LogDebug("Refresh JWT token...");
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            var loginResult = await authorizationService.GenerateNewJwtToken(accessToken, jwt);
                            var refreshedJwt = loginResult.JwtToken;

                            var httpHeadersService = scope.Resolve<IHttpHeadersService>();
                
                            _logger.LogDebug($"Cookie with JWT token was refreshed. Expiration time: {jwtAuthService.GetTokenExpirationTime(refreshedJwt)}");
                            context.Request.Headers.Authorization = $"Bearer {refreshedJwt}";
                
                            cookiesService.Append(
                                context,
                                HttpCookieKeyEnum.JwtToken, 
                                refreshedJwt, 
                                DateTimeOffset.UtcNow.AddDays(30)
                            );
                            httpHeadersService.Append(HttpHeaderKeyEnum.JwtToken, refreshedJwt);
                        }
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
        catch (ExpiredJwtTokenException e)
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
