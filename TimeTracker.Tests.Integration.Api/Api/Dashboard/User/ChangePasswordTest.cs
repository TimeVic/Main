using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class ChangePasswordTest : BaseTest
{
    private const string Url = "/dashboard/user/change-password";
    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly IPasswordService _passwordService;

    public ChangePasswordTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync("CurrentPass123").Result;
        _passwordService = ServiceProvider.GetRequiredService<IPasswordService>();
    }

    [Fact]
    public async Task ShouldChangePassword()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new ChangePasswordRequest
        {
            CurrentPassword = "CurrentPass123",
            NewPassword = "NewPassword123"
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_user);
        Assert.True(_passwordService.ValidatePassword(_user, "NewPassword123"));
    }

    [Fact]
    public async Task ShouldRejectIncorrectCurrentPassword()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword123",
            NewPassword = "NewPassword123"
        });

        var data = await response.GetJsonResponseAsync<object>();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(new UserNotAuthorizedException().GetTypeName(), data.ErrorCode);
    }

    [Fact]
    public async Task AnonymousUserCannotChangePassword()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new ChangePasswordRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
