using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Services.Auth.SocialLogin.Dto;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Auth.SocialLogin.Providers;

public class AppleSocialLoginProviderService : ASocialLoginProvider, IAppleSocialLoginProviderService
{
    private const string AuthUrl = "https://appleid.apple.com/auth/authorize";
    private const string GetAccessTokenUrl = "https://appleid.apple.com/auth/token";
    private const string GetUserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
    
    private readonly ILogger<AppleSocialLoginProviderService> _logger;
    
    private readonly string _scope = "name email";

    private readonly string _privateKeyString;
    private readonly string _teamId;
    private readonly string _keyId;
    private readonly SigningCredentials _credentials;

    public AppleSocialLoginProviderService(
        IConfiguration configuration,
        ILogger<AppleSocialLoginProviderService> logger,
        IHttpCookiesService httpCookiesService
    ): base("Apple", configuration, httpCookiesService, logger)
    {
        _logger = logger;
        _teamId = configuration.GetValue<string>($"SocialLogin:Apple:TeamId")!;
        _keyId = configuration.GetValue<string>($"SocialLogin:Apple:KeyId")!;
        var keyFileName = configuration.GetValue<string>($"SocialLogin:Apple:KeyFileName")!;
        
        var credentialsDirectory = configuration.GetValue<string>("App:Storage:CredentialsDirectory");
        var keyFilePath = Path.Combine(AssemblyUtils.GetAssemblyPath(), credentialsDirectory + keyFileName);
        if (!File.Exists(keyFilePath))
        {
            throw new Exception($"Apple OAuth Login provider key file not found: {keyFilePath}");
        }
        
        logger.LogInformation($"Apple OAuth key initialization: {keyFilePath}");
        _privateKeyString = File.ReadAllText(keyFilePath);
        var keyEcdsa = ECDsa.Create();
        keyEcdsa.ImportFromPem(_privateKeyString.ToCharArray());
        
        var securityKey = new ECDsaSecurityKey(keyEcdsa) { KeyId = _keyId };
        _credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);
        logger.LogInformation($"Apple OAuth credentials initialized...");
    }
    
    protected override string GetLoginUrl(string state)
    {
        return AuthUrl +
               $"?response_type=code id_token" +
               $"&response_mode=form_post" +
               $"&client_id={_clientId}" +
               $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
               $"&scope={Uri.EscapeDataString(_scope)}" +
               $"&state={state}";
    }

    string CreateClientSecret()
    {
        // var securityKey = new X509SecurityKey(new X509Certificate2(Encoding.UTF8.GetBytes(_privateKeyString)));
        // var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        
        _logger.LogInformation($"Create Apple JwtSecurityToken");
        
        var claims = new ClaimsIdentity([
            new Claim("sub", _clientId)
        ]);
        var token = new JwtSecurityToken(
            issuer: _teamId,
            audience: "https://appleid.apple.com",
            claims: claims.Claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public override async Task<UserInfoDto> HandleCallback(string code, string? state)
    {
        _logger.LogInformation($"Apple OAuth callback processing started...");
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
            AppleId = userInfo.Id,
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
        var id = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var email = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        var name = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
        return new UserInfoDto()
        {
            AppleId = id,
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
        var clientSecret = CreateClientSecret();
        
        var logMetadata = new Dictionary<string, object>() {
            { "ClientId", _clientId },
            { "CallbackUrl", _callbackUrl },
            { "Code", code },
        };
        _logger.LogInformation($"Retrieve access token from Apple OAuth provider...", logMetadata);
        
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, GetAccessTokenUrl);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _callbackUrl,
            
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
                var responseDto = JsonHelper.DeserializeObject<OauthLoginDto>(tokenJson)!;
                _logger.LogInformation($"Access token received... Scope: {responseDto.Scope}");
                return responseDto;
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
    
    private async Task<AppleUserInfoDto?> GetUserInfo(OauthLoginDto loginResponse)
    {
        try
        {
            _logger.LogInformation("Parse Apple SocialLogin JWT token...");
            
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(loginResponse.IdToken);

            var logMetadata = new Dictionary<string, object>() {};
            foreach (var claim in jwt.Claims)
                logMetadata.Add($"ClaimType: {claim.Type}", claim.Value);
            _logger.LogInformation("Apple SocialLogin data received", logMetadata);
            
            var id = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            if (id == null)
            {
                _logger.LogError($"AppleLogin. User ID was not received from ID token: {loginResponse.IdToken}");
                throw new DataValidationException();
            }
            if (email == null)
            {
                _logger.LogError($"AppleLogin. User Email was not received from ID token: {loginResponse.IdToken}");
                throw new DataValidationException();
            }
            return await Task.FromResult(new AppleUserInfoDto() {
                Id = id, 
                Email = email,
                Name = name ?? string.Empty
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return null;
    }
}
