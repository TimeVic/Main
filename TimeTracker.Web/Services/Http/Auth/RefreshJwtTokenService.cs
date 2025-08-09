using Fluxor;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Helpers;
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
    
    private readonly SemaphoreSlim _jwtReceivingLock = new(1, 1);
    
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
    
    public string? GetJwt()
    {
        WaitUntilJwtRefreshed();
        if (string.IsNullOrEmpty(_jwtToken))
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            _jwtToken = store?.Value.JwtToken?.Trim();
        }

        return _jwtToken;
    }
        
    public string? GetAccessToken()
    {
        WaitUntilJwtRefreshed();
        var store = _serviceProvider.GetService<IState<AuthState>>();
        return store?.Value.AccessToken?.Trim();
    }
    
    public async Task<string?> TryRefreshToken()
    {
        var jwtToken = GetJwt();
        if (string.IsNullOrEmpty(jwtToken))
        {
            return null;
        }

        var jwtExpirationTime = JwtHelper.GetExpiryTimestamp(jwtToken);
        Debug.Log($"JWT expiration time: {jwtExpirationTime}", DateTime.UtcNow);
        var diff = jwtExpirationTime - DateTime.UtcNow;
        if (diff.TotalMinutes <= 2)
            return await RequestNewToken();
        return GetJwt();
    }

    private async Task<string> RequestNewToken()
    {
        _logger.LogInformation("Try to re-new JWT token...");
        await _jwtReceivingLock.WaitAsync();
        
        var refreshResult = await _httpClient.RequestAsync<RefreshTokenResponseDto>(
            ApiUrl.RefreshToken, 
            new RefreshTokenRequest()
            {
                JwtToken = _jwtToken,
                AccessToken = GetAccessToken() ?? ""
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
        _dispatcher.Dispatch(new PersistDataAction());
        
        _logger.LogInformation("Token updated release lock");
        _jwtReceivingLock.Release();
        return _jwtToken;
    }

    private void WaitUntilJwtRefreshed()
    {
        _jwtReceivingLock.WaitAsync().Wait();
        _jwtReceivingLock.Release();
    }
}
