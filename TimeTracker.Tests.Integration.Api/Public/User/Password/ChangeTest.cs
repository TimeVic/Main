using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.User.Password;

public class ChangeTest: BaseTest
{
    private readonly string Url = "/user/password/change";
    
    private readonly IJwtAuthService _jwtService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly UserEntity _user;
    private readonly string _expectedPassword;
    private readonly IPasswordService _passwordService;

    public ChangeTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _resetPasswordService = ServiceProvider.GetRequiredService<IResetPasswordService>();
        _passwordService = ServiceProvider.GetRequiredService<IPasswordService>();

        _expectedPassword = "somePass123";
        _user = UserSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldChange()
    {
        var expectedPassword = "somePass123";
        var actualRequest = await _resetPasswordService.Generate(_user);
        var response = await PostRequestAsAnonymousAsync(Url, new ResetPasswordStep2Request()
        {
            VerficationToken = actualRequest.VerificationToken,
            Password = expectedPassword,
            ReCaptcha = "captcha"
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_user);
        Assert.True(_passwordService.ValidatePassword(_user, expectedPassword));
    }
    
    [Fact]
    public async Task ShouldNotChangeIfIncorrectToken()
    {
        var actualRequest = await _resetPasswordService.Generate(_user);
        var response = await PostRequestAsAnonymousAsync(Url, new ResetPasswordStep2Request()
        {
            VerficationToken = actualRequest.VerificationToken + "a",
            Password = "somePass123",
            ReCaptcha = "captcha"
        });
        var responseData = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), responseData.Type);
    }
}
