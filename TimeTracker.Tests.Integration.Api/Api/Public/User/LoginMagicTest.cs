using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class LoginMagicTest : BaseTest
{
    private readonly string _sendUrl = "/user/login/magic";
    private readonly string _verifyUrl = "/user/login/magic/verify";

    private readonly IJwtAuthService _jwtService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly UserEntity _user;

    public LoginMagicTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _authorizationService = ServiceProvider.GetRequiredService<IAuthorizationService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();

        _user = UserSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldSendMagicLink()
    {
        var response = await PostRequestAsAnonymousAsync(_sendUrl, new LoginMagicRequest()
        {
            Email = _user.Email,
            ReCaptcha = "captcha"
        });
        response.EnsureSuccessStatusCode();
        await QueueProcess(QueueChannel.Notifications);
        Assert.True(SmtpClientServiceMock.IsEmailSent);
    }

    [Fact]
    public async Task ShouldNotSendIfUserNotFound()
    {
        var response = await PostRequestAsAnonymousAsync(_sendUrl, new LoginMagicRequest()
        {
            Email = "nonexistent@example.com",
            ReCaptcha = "captcha"
        });
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.ErrorCode);
    }

    [Fact]
    public async Task ShouldVerifyTokenAndLogin()
    {
        var magicToken = await _authorizationService.GenerateMagicToken(_user.Email);
        await FlushDbChanges();

        var response = await PostRequestAsAnonymousAsync(_verifyUrl, new VerifyMagicTokenRequest()
        {
            Token = magicToken.Token
        });
        response.EnsureSuccessStatusCode();
        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();

        Assert.True(_jwtService.IsValidJwt(responseData.JwtToken));
        Assert.NotEmpty(responseData.AccessToken);
        Assert.NotEqual(Guid.Empty, responseData.User.Id);
        Assert.Equal(_user.Email, responseData.User.Email);
        Assert.NotNull(responseData.User.DefaultWorkspace);

        var actualAccessToken = await _accessTokenDao.GetByToken(responseData.AccessToken);
        Assert.NotNull(actualAccessToken);
        Assert.Contains(actualAccessToken.JwtTokens, item => item.Token == responseData.JwtToken);
    }

    [Fact]
    public async Task ShouldNotVerifyWithInvalidToken()
    {
        var response = await PostRequestAsAnonymousAsync(_verifyUrl, new VerifyMagicTokenRequest()
        {
            Token = "invalid-token-value"
        });
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotVerifyExpiredToken()
    {
        var magicToken = await _authorizationService.GenerateMagicToken(_user.Email);
        magicToken.ExpirationTime = DateTime.UtcNow.AddHours(-1);
        await FlushDbChanges();

        var response = await PostRequestAsAnonymousAsync(_verifyUrl, new VerifyMagicTokenRequest()
        {
            Token = magicToken.Token
        });
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.ErrorCode);
    }

    [Fact]
    public async Task ShouldBeOneTimeUse()
    {
        var magicToken = await _authorizationService.GenerateMagicToken(_user.Email);
        await FlushDbChanges();

        var firstResponse = await PostRequestAsAnonymousAsync(_verifyUrl, new VerifyMagicTokenRequest()
        {
            Token = magicToken.Token
        });
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await PostRequestAsAnonymousAsync(_verifyUrl, new VerifyMagicTokenRequest()
        {
            Token = magicToken.Token
        });
        var responseData = await secondResponse.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.ErrorCode);
    }
}
