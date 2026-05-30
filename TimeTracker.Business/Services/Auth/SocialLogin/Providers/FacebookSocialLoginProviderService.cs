using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Services.Auth.SocialLogin.Dto;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Auth.SocialLogin.Providers;

public class FacebookSocialLoginProviderService : ASocialLoginProvider, IFacebookSocialLoginProviderService
{
    private const string AuthUrl = "https://www.facebook.com/v18.0/dialog/oauth";
    private const string GetAccessTokenUrl = "https://graph.facebook.com/v18.0/oauth/access_token";
    private const string GetUserInfoUrl = "https://graph.facebook.com/me";
    
    private readonly string _scope = "openid email profile";
 
    private readonly ILogger<FacebookSocialLoginProviderService> _logger;
    private readonly IJwtAuthService _jwtAuthService;

    public FacebookSocialLoginProviderService(
        IConfiguration configuration,
        ILogger<FacebookSocialLoginProviderService> logger,
        IHttpCookiesService httpCookiesService,
        IJwtAuthService jwtAuthService
    ): base("Facebook", configuration, httpCookiesService, logger)
    {
        _logger = logger;
        _jwtAuthService = jwtAuthService;
    }
    
    protected override string GetLoginUrl(string state)
    {
        return AuthUrl +
               $"?client_id={_clientId}" +
               $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
               $"&response_type=code" +
               $"&scope=email,public_profile" +
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
        var result = await HandleIdToken(oauthLoginResponse.AccessToken);
        var (_, loginReturnUrl, registrationReturnUrl) = GetCookies();
        result.LoginReturnUrl = loginReturnUrl;
        result.RegistrationReturnUrl = registrationReturnUrl;
        return result;
    }
    
    public override async Task<UserInfoDto> HandleIdToken(string idToken)
    {
        string facebookId;
        string email;
        string[]? nameParts;
        if (_jwtAuthService.IsJwt(idToken)) 
        {
            var principal = await ValidateAndParseJwtToken(idToken);
            facebookId = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value;
            email = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? string.Empty;
            nameParts = principal.FindFirst("name")?.Value.Split(' ');
        }
        else
        {
            var userInfo = await GetUserInfo(idToken);
            if (userInfo == null)
            {
                throw new UserNotAuthorizedException();
            }
            facebookId = userInfo.Id;
            email = userInfo.Email;
            nameParts = userInfo.Name.Split(' ');
        }
        var firstName = nameParts?.FirstOrDefault() ?? string.Empty;
        var lastName = nameParts?.Skip(1).FirstOrDefault() ?? string.Empty;
        return new UserInfoDto()
        {
            FacebookId = facebookId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AccessToken = idToken
        };
    }
    
    private async Task<OauthLoginDto?> GetAccessTokenAsync(string code)
    {
        var tokenUrl = GetAccessTokenUrl +
                       $"?client_id={_clientId}" +
                       $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
                       $"&client_secret={_clientSecret}" +
                       $"&code={code}";

        try
        {
            var tokenJson = await _httpClient.GetStringAsync(tokenUrl);
            if (string.IsNullOrEmpty(tokenJson))
            {
                _logger.LogError($"Response can not be parsed: {tokenJson}");
                return null;
            }
            else
            {
                return JsonHelper.DeserializeObject<OauthLoginDto>(tokenJson)!;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }
    
    private async Task<FacebookUserInfoDto?> GetUserInfo(string accessToken)
    {
        var url = GetUserInfoUrl + $"?fields=id,name,email&access_token={accessToken}";
        var requestData = new HttpRequestMessage(HttpMethod.Get, url);
        try
        {
            var requestResponse = await _httpClient.SendAsync(requestData);
            var responseJson = await requestResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogError($"Response can not be parsed: {responseJson}");
                return null;
            }
            if (requestResponse.IsSuccessStatusCode)
            {
                return JsonHelper.DeserializeObject<FacebookUserInfoDto>(responseJson)!;
            }
            else
            {
                _logger.LogError($"Facebook social login error: {responseJson}");
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
