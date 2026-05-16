using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class UpdateSettingsTest : BaseTest
{
    private const string Url = "/dashboard/user/update-settings";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly IUserDao _userDao;

    public UpdateSettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateSettingsRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdateUserSettings()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateSettingsRequest
        {
            UserName = "Updated User",
            LanguageCode = "uk-UA"
        });
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        await FlushDbChanges(isClearSession: true);
        var persistedUser = await _userDao.GetById(_user.Id);

        Assert.Equal("Updated User", actualUser.UserName);
        Assert.NotNull(actualUser.Language);
        Assert.Equal("uk-UA", actualUser.Language.Code);
        Assert.Equal("Updated User", persistedUser!.UserName);
        Assert.Equal("uk-UA", persistedUser.Language.Code);
    }

    [Fact]
    public async Task ShouldReturnBadRequestIfLanguageDoesNotExist()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateSettingsRequest
        {
            UserName = "Updated User",
            LanguageCode = "zz"
        });
        var responseData = await response.GetJsonResponseAsync<object>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("RecordNotFoundException", responseData.ErrorCode);
    }
}
