using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.User.Registration;

public class Step2Test: BaseTest
{
    private readonly string Url = "/user/registration/step2";
    
    private readonly IQueueService _queueService;
    private readonly IRegistrationService _registrationService;
    private readonly IJwtAuthService _jwtService;
    private readonly IQueueDao _queueDao;

    public Step2Test(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _registrationService = ServiceProvider.GetRequiredService<IRegistrationService>();
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();

        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldActivatePendingUser()
    {
        var user = UserFactory.Generate();
        user = await _registrationService.CreatePendingUser(user.Email);
        
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep2Request()
        {
            Token = user.VerificationToken,
            Password = SecurityUtil.GeneratePassword(12),
            ReCaptcha = "aaa"
        });
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<RegistrationStep2ResponseDto>();
        Assert.True(_jwtService.IsValidJwt(responseData.JwtToken));
        Assert.Equal(user.Id, _jwtService.GetUserId(responseData.JwtToken));
        Assert.NotNull(responseData.User);
        Assert.NotNull(responseData.User.DefaultWorkspace);
        Assert.True(responseData.User.DefaultWorkspace.IsDefault);
        
        await _queueService.Process(QueueChannel.Notifications);
        Assert.True(EmailSendingService.IsEmailSent);
        Assert.Contains(EmailSendingService.SentMessages, message =>
        {
            return message.Body.Contains("is verified");
        });
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestIfBadPassword()
    {
        var user = UserFactory.Generate();
        user = await _registrationService.CreatePendingUser(user.Email);
        
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep2Request()
        {
            Token = user.VerificationToken,
            Password = SecurityUtil.GeneratePassword(3),
            ReCaptcha = "aaa"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
