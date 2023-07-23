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

public class ResetTest: BaseTest
{
    private readonly string Url = "/user/password/reset";
    
    private readonly IJwtAuthService _jwtService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly UserEntity _user;

    public ResetTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _resetPasswordService = ServiceProvider.GetRequiredService<IResetPasswordService>();

        _user = UserSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldCreateResetRequest()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new ResetPasswordStep1Request()
        {
            Email = _user.Email,
            ReCaptcha = "captcha"
        });
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task ShouldNotCreateIfExpired()
    {
        await _resetPasswordService.Generate(_user);
        
        var response = await PostRequestAsAnonymousAsync(Url, new ResetPasswordStep1Request()
        {
            Email = _user.Email,
            ReCaptcha = "captcha"
        });
        var responseData = await response.GetJsonErrorAsync();
        Assert.Equal(new TooManyRequestsException().GetTypeName(), responseData.Type);
    }
}
