using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User.Registration;

public class Step1Test: BaseTest
{
    private readonly string Url = "/user/registration/step1";
    
    private readonly IQueueService _queueService;
    private readonly IUserDao _userDao;

    public Step1Test(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
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

        await QueueProcess(QueueChannel.Notifications);
        Assert.Contains(GraylogClient.EmailLogs, item => item.EmailTo == user.Email);
    }

    [Fact]
    public async Task ShouldCreatePendingUserWithLandingLanguage()
    {
        var user = UserFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new RegistrationStep1Request()
        {
            Email = user.Email,
            ReCaptcha = "captcha",
            LanguageCode = "uk-UA"
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges(isClearSession: true);
        var actualUser = await _userDao.GetByEmail(user.Email);

        Assert.NotNull(actualUser);
        Assert.Equal("uk-UA", actualUser.Language.Code);
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
