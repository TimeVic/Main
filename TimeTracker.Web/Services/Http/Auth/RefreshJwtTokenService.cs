using Fluxor;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
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
        var store = _serviceProvider.GetService<IState<AuthState>>();
        return store?.Value.JwtToken?.Trim();
    }
        
    public string? GetAccessToken()
    {
        var store = _serviceProvider.GetService<IState<AuthState>>();
        return store?.Value.AccessToken?.Trim();
    }
    
    public async Task<string?> TryRefreshToken()
    {
        Debug.Log("Try Update JWT: ", GetJwt());
        if (string.IsNullOrEmpty(GetJwt()))
        {
            return null;
        }

        var jwtExpirationTime = JwtHelper.GetExpiryTimestamp(GetJwt());
        var diff = jwtExpirationTime - DateTime.UtcNow;
        if (diff.TotalMinutes <= 2)
            return await RequestNewToken();
        return string.Empty;
    }

    private async Task<string> RequestNewToken()
    {
        var refreshResult = await _httpClient.RequestAsync<RefreshTokenResponseDto>(
            ApiUrl.RefreshToken, 
            new RefreshTokenRequest()
            {
                JwtToken = GetJwt() ?? "",
                AccessToken = GetAccessToken() ?? ""
            },
            HttpMethod.Post
        );
        _dispatcher.Dispatch(new SetJwtAction(refreshResult.JwtToken));
        _dispatcher.Dispatch(new PersistDataAction());
        return refreshResult.JwtToken;
    }
}
