using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http.Client;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Services.Http.Auth;

public class RefreshJwtTokenService
{
    private readonly CustomHttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshJwtTokenService> _logger;
    private readonly IDispatcher _dispatcher;

    private string _jwtToken;
    
    private TaskCompletionSource<bool>? _lockReleased = null;
    
    public RefreshJwtTokenService(
        CustomHttpClient httpClient,
        IServiceProvider serviceProvider,
        ILogger<RefreshJwtTokenService> logger,
        IDispatcher dispatcher
    )
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dispatcher = dispatcher;
    }
    
    public async Task<string?> GetJwt()
    {
        await WaitUntilUnlockedAsync();
        if (string.IsNullOrEmpty(_jwtToken))
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            _jwtToken = store?.Value.JwtToken.Trim() ?? string.Empty;
        }

        return _jwtToken;
    }
        
    public async Task<string?> GetAccessToken()
    {
        var store = _serviceProvider.GetService<IState<AuthState>>();
        return store?.Value.AccessToken?.Trim();
    }
    
    public async Task<string?> TryRefreshToken()
    {
        var jwtToken = await GetJwt();
        if (string.IsNullOrEmpty(jwtToken))
        {
            return null;
        }

        var jwtExpirationTime = JwtHelper.GetExpiryTimestamp(jwtToken);
        var diff = jwtExpirationTime - DateTime.UtcNow;
        
        if (diff.TotalMinutes <= 2)
            return await RequestNewToken();
        return await GetJwt();
    }

    private async Task<string> RequestNewToken()
    {
        _logger.LogInformation("Try to re-new JWT token...");
        StartLock();
        
        _logger.LogInformation("Call HTTP request to refresh JWT token");
        try
        {
            var refreshResult = await _httpClient.RequestAsync<RefreshTokenResponseDto>(
                ApiUrl.RefreshToken,
                new RefreshTokenRequest()
                {
                    JwtToken = _jwtToken,
                    AccessToken = await GetAccessToken() ?? string.Empty
                },
                HttpMethod.Post
            );
            if (refreshResult == null)
            {
                _logger.LogInformation("Token can not be refreshed");
                throw new ServerException();
            }
            
            _jwtToken = refreshResult.JwtToken;
            _dispatcher.Dispatch(new SetJwtAction(refreshResult.JwtToken));
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"JWT Token can not be refreshed. Logout: {e.Message}");
            throw;
        }
        finally
        {
            _dispatcher.Dispatch(new PersistDataAction());
            ReleaseLock();
        } 
        return _jwtToken;
    }

    private void StartLock()
    {
        _logger.LogInformation("Start JWT receiving lock");
        _lockReleased = new TaskCompletionSource<bool>();
    }

    private void ReleaseLock()
    {
        _lockReleased?.TrySetResult(true);
        _lockReleased = null;
        _logger.LogInformation("Lock released");
    }

    private async Task WaitUntilUnlockedAsync()
    {
        if (_lockReleased != null)
        {
            _logger.LogInformation("Wait when lock will be released");
            await _lockReleased.Task;
        }
    }
}
