using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Services.Auth.SocialLogin.Dto;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Auth.SocialLogin;

public abstract class ASocialLoginProvider : ISocialLoginProvider
{
    private readonly IHttpCookiesService _httpCookiesService;
    private readonly ILogger<ASocialLoginProvider> _logger;
    protected readonly string _clientId;
    protected readonly string _clientSecret;
    protected readonly string _callbackUrl;
    private readonly string _validationIssuer;
    private readonly string _publicKeysUrl;
    protected readonly string[] _validationAudiences;
    
    protected readonly HttpClient _httpClient = new();

    public ASocialLoginProvider(
        string providerName,
        IConfiguration configuration,
        IHttpCookiesService httpCookiesService,
        ILogger<ASocialLoginProvider> logger
    )
    {
        _httpCookiesService = httpCookiesService;
        _logger = logger;

        _clientId = configuration.GetValue<string>($"SocialLogin:{providerName}:ClientId")!;
        _clientSecret = configuration.GetValue<string>($"SocialLogin:{providerName}:ClientSecret")!;
        _validationIssuer = configuration.GetValue<string>($"SocialLogin:{providerName}:ValidIssuer")!;
        _validationAudiences = configuration.GetSection($"SocialLogin:{providerName}:ValidAudiences").Get<string[]>()!;
        _publicKeysUrl = configuration.GetValue<string>($"SocialLogin:{providerName}:IssuerPublicKeysUrl")!;
        var callbackPathTemplate = configuration.GetValue<string>($"SocialLogin:CallbackPathTemplate")!;
        var backendUrl = configuration.GetValue<string>("App:BackendUrl")!;

        _callbackUrl = string.Format(callbackPathTemplate, backendUrl, providerName);;
        // $"https://localhost:7146/api/Social/Login/Callback/{providerName}";
        // string.Format(callbackPathTemplate, backendUrl, providerName);
    }

    protected void SetCookies(string state, Uri? loginReturnUrl = null, Uri? registrationReturnUrl = null)
    {
        var lifetime = DateTimeOffset.UtcNow.AddMinutes(5);
        _httpCookiesService.Append(HttpCookieKeyEnum.SocialLoginState, state, lifetime);
        _httpCookiesService.Append(HttpCookieKeyEnum.SocialLoginReturnUrl, loginReturnUrl?.ToString() ?? string.Empty);
        _httpCookiesService.Append(HttpCookieKeyEnum.SocialLoginRegistrationReturnUrl, registrationReturnUrl?.ToString() ?? string.Empty);
    }

    protected (string state, Uri? loginReturnUrl, Uri? registrationReturnUrl) GetCookies()
    {
        var state = _httpCookiesService.Get(HttpCookieKeyEnum.SocialLoginState);
        var loginReturnUrlString = _httpCookiesService.Get(HttpCookieKeyEnum.SocialLoginReturnUrl);
        var registrationReturnUrlString = _httpCookiesService.Get(HttpCookieKeyEnum.SocialLoginRegistrationReturnUrl);
        
        Uri.TryCreate(loginReturnUrlString, UriKind.Absolute, out var loginReturnUrl);
        Uri.TryCreate(registrationReturnUrlString, UriKind.Absolute, out var registrationReturnUrl);
        if (string.IsNullOrEmpty(state))
            throw new DataValidationException("Incorrect login state");
        return (state, loginReturnUrl, registrationReturnUrl);
    }

    protected void ValidateState(string stateToValidate)
    {
        _logger.LogInformation("Validate received state...");
        var (state, loginReturnUrl, registrationReturnUrl) = GetCookies();
        if (state != stateToValidate)
            throw new DataValidationException("Incorrect login state");
    }
    
    public string BuildLoginUrl(Uri? loginReturnUrl = null, Uri? registrationReturnUrl = null)
    {
        _logger.LogInformation("Build return URL...");
        var state = Guid.NewGuid().ToString();
        SetCookies(state, loginReturnUrl, registrationReturnUrl);
        var loginUrl = GetLoginUrl(state);
        return loginUrl;
    }
    
    private async Task<SecurityKey> GetSigningKey(string kid)
    {
        _logger.LogInformation("Receive Sign In issuer key to JWT validation");
        try
        {
            var jwksJson = await _httpClient.GetStringAsync(_publicKeysUrl);

            var jwks = JsonDocument.Parse(jwksJson);
            var keys = jwks.RootElement.GetProperty("keys");
            foreach (var key in keys.EnumerateArray())
            {
                if (key.GetProperty("kid").GetString() == kid)
                {
                    var n = Base64UrlEncoder.DecodeBytes(key.GetProperty("n").GetString());
                    var e = Base64UrlEncoder.DecodeBytes(key.GetProperty("e").GetString());

                    var rsa = RSA.Create();
                    rsa.ImportParameters(new RSAParameters { Modulus = n, Exponent = e });

                    return new RsaSecurityKey(rsa) { KeyId = kid };
                }
            }

            throw new Exception("Key with provided KID not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Issuer public keys was not received: {kid} - {e.Message}");
            throw new DataValidationException("Issuer sign in keys receiving error");
        }
    }

    
    protected async Task<ClaimsPrincipal> ValidateAndParseJwtToken(string identityToken)
    {
        var logMetadata = new Dictionary<string, object>() {
            {"ValidAudiences", string.Join(", ", _validationAudiences)},
            {"ValidIssuer", _validationIssuer},
        };
        _logger.LogInformation($"Verify received identity token: {identityToken}", logMetadata);
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(identityToken);
            var kid = jwt.Header.Kid;

            var signingKey = await GetSigningKey(kid);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _validationIssuer,
                
                ValidateAudience = _validationAudiences.Any(),
                ValidAudiences = _validationAudiences,
                
                ValidateLifetime = true,
                
                IssuerSigningKey = signingKey,
                ValidateIssuerSigningKey = true,
            };
            
            SecurityToken validatedToken;
            var principal = handler.ValidateToken(identityToken, parameters, out validatedToken);
            LogPrincipals(principal);
            return principal;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Identity token validation error: {e.Message}");
            throw new DataValidationException("Identity token validation error");
        }
    }
    
    private void LogPrincipals(ClaimsPrincipal principal)
    {
        var sb = new StringBuilder();
        var logMetadata = new Dictionary<string, object>() {};
        foreach (var identity in principal.Identities)
        {
            logMetadata.Add("AuthResult", $"Identity: {identity.AuthenticationType}, IsAuthenticated: {identity.IsAuthenticated}");
            sb.AppendLine($"Identity: {identity.AuthenticationType}, IsAuthenticated: {identity.IsAuthenticated}");
            foreach (var claim in identity.Claims)
            {
                logMetadata.Add($"ClaimType: {claim.Type}", claim.Value);
            }
        }
        _logger.LogInformation("SocialLogin. Received and validated principals", logMetadata);
    }

    
    protected abstract string GetLoginUrl(string state);

    public abstract Task<UserInfoDto> HandleCallback(string code, string? state);
    
    public abstract Task<UserInfoDto> HandleIdToken(string idToken);
}
