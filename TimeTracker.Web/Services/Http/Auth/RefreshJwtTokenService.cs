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
    private readonly IDispatcher _dispatcher;

    private string _jwtToken;
    
    public RefreshJwtTokenService(
        CustomHttpClient httpClient,
        IServiceProvider serviceProvider,
        IDispatcher dispatcher
    )
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
    }
    
    public string? GetJwt()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            _jwtToken = store?.Value.JwtToken?.Trim();
        }

        return _jwtToken;
    }
        
    public string? GetAccessToken()
    {
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
        Debug.Log("jwtExpirationTime", jwtExpirationTime);
        var diff = jwtExpirationTime - DateTime.UtcNow;
        Debug.Log("diff.TotalMinutes", diff.TotalMinutes);
        if (diff.TotalMinutes <= 2)
            return await RequestNewToken();
        return GetJwt();
    }

    private async Task<string> RequestNewToken()
    {
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
            throw new ServerException();
        }

        _jwtToken = refreshResult.JwtToken;
        Debug.Log("JwtHelper.GetExpiryTimestamp(jwtToken)", JwtHelper.GetExpiryTimestamp(refreshResult.JwtToken));
        _dispatcher.Dispatch(new SetJwtAction(refreshResult.JwtToken));
        _dispatcher.Dispatch(new PersistDataAction());
        return _jwtToken;
    }
}
