using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Services.Auth.SocialLogin.Dto;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Auth.SocialLogin.Providers;

public class GoogleSocialLoginProviderService : ASocialLoginProvider, IGoogleSocialLoginProviderService
{
    private const string AuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GetAccessTokenUrl = "https://oauth2.googleapis.com/token";
    private const string GetUserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
    
    private readonly ILogger<GoogleSocialLoginProviderService> _logger;
    private readonly string _scope = "openid email profile";

    public GoogleSocialLoginProviderService(
        IConfiguration configuration,
        ILogger<GoogleSocialLoginProviderService> logger,
        IHttpCookiesService httpCookiesService
    ): base("Google", configuration, httpCookiesService, logger)
    {
        _logger = logger;
    }
    
    protected override string GetLoginUrl(string state)
    {
        return AuthUrl +
               $"?client_id={_clientId}" +
               $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString(_scope)}" +
               $"&state={state}";
    }

    public override async Task<UserInfoDto> HandleCallback(string code, string? state)
    {
        ValidateState(state ?? string.Empty);
        var oauthLoginResponse = await GetAccessTokenAsync(code);
        if (oauthLoginResponse == null)
        {
            throw new UserNotAuthorizedException();
        }
        var userInfo = await GetUserInfo(oauthLoginResponse);
        if (userInfo == null)
        {
            throw new UserNotAuthorizedException();
        }
        var (_, loginReturnUrl, registrationReturnUrl) = GetCookies();
        return new UserInfoDto()
        {
            GoogleId = userInfo.Id,
            Email = userInfo.Email,
            FirstName = userInfo.GivenName,
            LastName = userInfo.FamilyName,
            AccessToken = oauthLoginResponse.AccessToken,
            RefreshToken = oauthLoginResponse.RefreshToken,
            LoginReturnUrl = loginReturnUrl,
            RegistrationReturnUrl = registrationReturnUrl
        };
    }

    public override async Task<UserInfoDto> HandleIdToken(string idToken)
    {
        var principal = await ValidateAndParseJwtToken(idToken);
        var googleId = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var email = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        var name = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
        return new UserInfoDto()
        {
            GoogleId = googleId,
            Email = email!,
            FirstName = name ?? string.Empty,
            LastName = string.Empty,
            AccessToken = string.Empty,
            RefreshToken = string.Empty,
            LoginReturnUrl = null
        };
    }
    
    private async Task<OauthLoginDto?> GetAccessTokenAsync(string code)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, GetAccessTokenUrl);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = _callbackUrl,
            ["grant_type"] = "authorization_code"
        });

        var tokenResponse = await _httpClient.SendAsync(tokenRequest);
        try
        {
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(tokenJson))
            {
                _logger.LogError($"Response can not be parsed: {tokenJson}");
                return null;
            }
            if (tokenResponse.IsSuccessStatusCode)
            {
                return JsonHelper.DeserializeObject<OauthLoginDto>(tokenJson)!;
            }
            else
            {
                _logger.LogError($"Google social login error: {tokenJson}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }
    
    private async Task<GoogleUserInfoDto?> GetUserInfo(OauthLoginDto loginResponse)
    {
        var requestData = new HttpRequestMessage(HttpMethod.Get, GetUserInfoUrl);
        requestData.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        try
        {
            var requestResponse = await _httpClient.SendAsync(requestData);
            var responseJson = await requestResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogError($"Response can not be parsed: {responseJson}");
                return null;
            }
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogError($"Response can not be parsed: {responseJson}");
                return null;
            }
            if (requestResponse.IsSuccessStatusCode)
            {
                return JsonHelper.DeserializeObject<GoogleUserInfoDto>(responseJson)!;
            }
            else
            {
                _logger.LogError($"Google social login error: {responseJson}");
            }
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }
}
