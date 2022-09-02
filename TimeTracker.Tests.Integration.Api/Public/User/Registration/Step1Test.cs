using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.User.Registration;

public class Step1Test: BaseTest
{
    private readonly string Url = "/user/registration/step1";
    
    private readonly IQueueService _queueService;

    public Step1Test(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
    }

    [Fact]
    public async Task ShouldCreatePendingUserAndSendEmail()
    {
        var user = UserFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep1Request()
        {
            Email = user.Email,
            ReCaptcha = "captcha"
        });
        response.EnsureSuccessStatusCode();

        await _queueService.Process(QueueChannel.Notifications);
        Assert.True(EmailSendingService.IsEmailSent);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestIfIncorrectEmail()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep1Request()
        {
            Email = "fake",
            ReCaptcha = "captcha"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestIfUserExists()
    {
        var actualUser = await UserSeeder.CreateActivatedAsync();
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep1Request()
        {
            Email = actualUser.Email,
            ReCaptcha = "captcha"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
