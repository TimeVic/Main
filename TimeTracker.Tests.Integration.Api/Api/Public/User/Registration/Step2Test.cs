using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User.Registration;

public class Step2Test: BaseTest
{
    private readonly string Url = "/user/registration/step2";
    
    private readonly IQueueService _queueService;
    private readonly IRegistrationService _registrationService;
    private readonly IJwtAuthService _jwtService;
    private new readonly IQueueDao _queueDao;
    private readonly IUserAccessTokenDao _accessTokenDao;

    public Step2Test(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _registrationService = ServiceProvider.GetRequiredService<IRegistrationService>();
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();

        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldActivatePendingUser()
    {
        var user = UserFactory.Generate();
        user = await _registrationService.CreatePendingUser(user.Email);
        
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep2Request()
        {
            Token = user.VerificationToken!,
            Password = SecurityUtil.GeneratePassword(12),
            ReCaptcha = "aaa"
        });
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<RegistrationStep2ResponseDto>();
        var jwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessToken = response.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());
        Assert.True(_jwtService.IsValidJwt(jwtToken!));
        Assert.Equal(user.Id, _jwtService.GetUserId(jwtToken!));
        Assert.NotEmpty(accessToken);
        Assert.Empty(responseData.JwtToken);
        Assert.Empty(responseData.AccessToken);
        Assert.NotNull(responseData.User);
        Assert.NotNull(responseData.User.DefaultWorkspace);
        Assert.True(responseData.User.DefaultWorkspace.IsDefault);
        
        await QueueProcess(QueueChannel.Notifications);
        Assert.Contains(GraylogClient.EmailLogs, message =>
        {
            return message.EmailBody.Contains("is verified");
        });
        
        var actualAccessToken = await _accessTokenDao.GetByToken(accessToken!);
        Assert.NotNull(actualAccessToken);
        Assert.Contains(actualAccessToken.JwtTokens, item => item.Token == jwtToken);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestIfBadPassword()
    {
        var user = UserFactory.Generate();
        user = await _registrationService.CreatePendingUser(user.Email);
        
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep2Request()
        {
            Token = user.VerificationToken!,
            Password = SecurityUtil.GeneratePassword(3),
            ReCaptcha = "aaa"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
