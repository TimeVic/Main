using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.User;

public class LoginTest: BaseTest
{
    private readonly string Url = "/user/login";
    
    private readonly IJwtAuthService _jwtService;

    public LoginTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
    }

    [Fact]
    public async Task ShouldLogin()
    {
        var expectedPassword = "test password";
        var user = await UserSeeder.CreateActivatedAsync(expectedPassword);
        var response = await PostRequestAsAnonymousAsync(Url, new LoginRequest()
        {
            Email = user.Email,
            Password = expectedPassword,
            ReCaptcha = "captcha"
        });
        response.EnsureSuccessStatusCode();
        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();

        Assert.True(_jwtService.IsValidJwt(responseData.Token));
        Assert.True(responseData.User.Id > 0);
        Assert.NotEmpty(responseData.User.Email);
    }
}
