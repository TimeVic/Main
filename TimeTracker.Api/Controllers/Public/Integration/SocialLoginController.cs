using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.Integrations;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Mvc.Controllers;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Auth.SocialLogin;
using TimeTracker.Business.Services.Auth.SocialLogin.Dto;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.Integration;

[Produces("application/json")]
[Route("integration/social/")]
[ApiController]
public class SocialLoginController : MainApiControllerBase
{
    private const string DashboardPath = "/board";
    private const string SignInPath = "/login";

    private readonly ISocialLoginProviderService _socialLoginProviderService;
    private readonly IUserDao _userDao;
    private readonly IUserSocialLoginDao _userSocialLoginDao;
    private readonly IRegistrationService _registrationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpCookiesService _httpCookiesService;
    private readonly IUrlService _urlService;
    private readonly ILogger<SocialLoginController> _logger;

    public SocialLoginController(
        ILifetimeScope scope,
        ISocialLoginProviderService socialLoginProviderService,
        IUserDao userDao,
        IUserSocialLoginDao userSocialLoginDao,
        IRegistrationService registrationService,
        IAuthorizationService authorizationService,
        IHttpCookiesService httpCookiesService,
        IUrlService urlService,
        ILogger<SocialLoginController> logger
    ): base(scope)
    {
        _socialLoginProviderService = socialLoginProviderService;
        _userDao = userDao;
        _userSocialLoginDao = userSocialLoginDao;
        _registrationService = registrationService;
        _authorizationService = authorizationService;
        _httpCookiesService = httpCookiesService;
        _urlService = urlService;
        _logger = logger;
    }

    [HttpGet("[action]/{providerName}")]
    public IActionResult SignIn(
        [FromRoute]string providerName,
        [FromQuery] Uri? returnUrl = null,
        [FromQuery] Uri? registrationReturnUrl = null,
        [FromQuery] Uri? errorReturnUrl = null
    )
    {
        var providerType = GetProviderType(providerName);
        var provider = _socialLoginProviderService.Provide(providerType);

        _httpCookiesService.Append(HttpCookieKeyEnum.SocialLoginErrorReturnUrl, errorReturnUrl?.ToString() ?? string.Empty);
        return Redirect(provider.BuildLoginUrl(returnUrl, registrationReturnUrl));
    }

    [HttpGet("SignIn/Mobile/{providerName}/{idToken}")]
    public async Task<IActionResult> MobileSignIn(
        [FromRoute]string providerName,
        [FromRoute]string idToken
    )
    {
        var providerType = GetProviderType(providerName);
        var provider = _socialLoginProviderService.Provide(providerType);
        var socialInfo = await provider.HandleIdToken(idToken);
        var user = await FindOrCreateUserAsync(socialInfo);

        await UpdateSocialLoginInfoAsync(user, socialInfo);

        var loginResult = await _authorizationService.Login(user);
        return JsonSuccess(new MobileSignInResponse
        {
            AccessToken = loginResult.AccessToken,
            JwtToken = loginResult.JwtToken,
        });
    }

    [HttpGet("Callback/{providerName}")]
    public async Task<IActionResult> GetCallback([FromRoute]string providerName, [FromQuery]string code, [FromQuery]string state)
    {
        return await ProcessWebCallback(providerName, code, state);
    }

    [HttpPost("Callback/{providerName}")]
    public async Task<IActionResult> PostCallback(
        [FromRoute]string providerName,
        [FromForm]string state,
        [FromForm]string? code = null,
        [FromForm]string? user = null,
        [FromForm]string? error = null
    )
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Social login provider returned error: {Error}", error);
        }

        if (!string.IsNullOrWhiteSpace(user))
        {
            _logger.LogInformation("Received social login user payload from callback.");
        }

        if (string.IsNullOrEmpty(code))
        {
            return RedirectToSocialLoginErrorUrl();
        }

        return await ProcessWebCallback(providerName, code, state);
    }

    private async Task<IActionResult> ProcessWebCallback(string providerName, string code, string state)
    {
        var providerType = GetProviderType(providerName);
        var provider = _socialLoginProviderService.Provide(providerType);
        var socialInfo = await provider.HandleCallback(code, state);
        var user = await FindOrCreateUserAsync(socialInfo);

        await UpdateSocialLoginInfoAsync(user, socialInfo);

        var loginResult = await _authorizationService.Login(user);
        _httpCookiesService.AppendAuthCookies(loginResult.AccessToken, loginResult.JwtToken);

        var redirectUrl = socialInfo.LoginReturnUrl ?? GetDashboardUri();
        redirectUrl = redirectUrl.AddOrUpdateQueryParam("isSuccessLogin", "true");
        redirectUrl = redirectUrl.AddOrUpdateQueryParam("provider", providerType.GetDescription());
        return Redirect(redirectUrl.ToString());
    }

    private async Task<UserEntity> FindOrCreateUserAsync(UserInfoDto socialInfo)
    {
        if (string.IsNullOrWhiteSpace(socialInfo.Email))
        {
            throw new DataValidationException("Social login email was not received");
        }

        _logger.LogInformation(
            "Try to find user by social login ids or email {Email}. GoogleId: {GoogleId}, FacebookId: {FacebookId}, AppleId: {AppleId}",
            socialInfo.Email,
            socialInfo.GoogleId,
            socialInfo.FacebookId,
            socialInfo.AppleId
        );

        var socialLogin = await _userSocialLoginDao.GetByProviderIdsAsync(
            socialInfo.GoogleId,
            socialInfo.FacebookId,
            socialInfo.AppleId
        );
        var user = socialLogin?.User ?? await _userDao.GetByEmail(socialInfo.Email);

        if (user == null)
        {
            return await _registrationService.CreateActivatedUserForSocialLogin(
                socialInfo.Email,
                BuildUserName(socialInfo)
            );
        }

        // Social login activates pending users that own the verified provider email.
        if (!user.IsActivated)
        {
            user = await _registrationService.CreateActivatedUserForSocialLogin(
                user.Email,
                BuildUserName(socialInfo)
            );
        }

        return user;
    }

    private async Task<UserSocialLoginEntity> UpdateSocialLoginInfoAsync(UserEntity user, UserInfoDto socialInfo)
    {
        var now = DateTime.UtcNow;
        var socialLogin = await _userSocialLoginDao.GetByUserAsync(user) ?? new UserSocialLoginEntity
        {
            User = user,
            CreatedAt = now
        };

        if (!string.IsNullOrWhiteSpace(socialInfo.GoogleId))
        {
            socialLogin.GoogleId = socialInfo.GoogleId;
            if (!string.IsNullOrWhiteSpace(socialInfo.AccessToken))
            {
                socialLogin.GoogleAccessToken = socialInfo.AccessToken;
            }

            if (!string.IsNullOrWhiteSpace(socialInfo.RefreshToken))
            {
                socialLogin.GoogleRefreshToken = socialInfo.RefreshToken;
            }

            socialLogin.GoogleConnectedAt = now;
        }

        if (!string.IsNullOrWhiteSpace(socialInfo.FacebookId))
        {
            socialLogin.FacebookId = socialInfo.FacebookId;
            if (!string.IsNullOrWhiteSpace(socialInfo.AccessToken))
            {
                socialLogin.FacebookAccessToken = socialInfo.AccessToken;
            }

            if (!string.IsNullOrWhiteSpace(socialInfo.RefreshToken))
            {
                socialLogin.FacebookRefreshToken = socialInfo.RefreshToken;
            }

            socialLogin.FacebookConnectedAt = now;
        }

        if (!string.IsNullOrWhiteSpace(socialInfo.AppleId))
        {
            socialLogin.AppleId = socialInfo.AppleId;
            if (!string.IsNullOrWhiteSpace(socialInfo.AccessToken))
            {
                socialLogin.AppleAccessToken = socialInfo.AccessToken;
            }

            if (!string.IsNullOrWhiteSpace(socialInfo.RefreshToken))
            {
                socialLogin.AppleRefreshToken = socialInfo.RefreshToken;
            }

            socialLogin.AppleConnectedAt = now;
        }

        socialLogin.UpdatedAt = now;
        user.SocialLoginInfo = await _userSocialLoginDao.SaveAsync(socialLogin);
        return user.SocialLoginInfo;
    }

    private IActionResult RedirectToSocialLoginErrorUrl()
    {
        var errorReturnUrlString = _httpCookiesService.Get(HttpCookieKeyEnum.SocialLoginErrorReturnUrl);
        if (Uri.TryCreate(errorReturnUrlString, UriKind.Absolute, out var errorReturnUrl))
        {
            return Redirect(errorReturnUrl.ToString());
        }

        return Redirect(_urlService.ToFrontendAbsoluteUrl(SignInPath));
    }

    private static SocialLoginProviderTypeEnum GetProviderType(string providerName)
    {
        if (!Enum.TryParse<SocialLoginProviderTypeEnum>(providerName, true, out var providerType))
        {
            throw new RecordNotFoundException();
        }

        return providerType;
    }

    private static string? BuildUserName(UserInfoDto socialInfo)
    {
        var parts = new[] { socialInfo.FirstName, socialInfo.LastName }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!.Trim());
        var userName = string.Join(" ", parts);
        return string.IsNullOrWhiteSpace(userName) ? null : userName;
    }

    private Uri GetDashboardUri()
    {
        return new Uri(_urlService.ToFrontendAbsoluteUrl(DashboardPath));
    }
}
