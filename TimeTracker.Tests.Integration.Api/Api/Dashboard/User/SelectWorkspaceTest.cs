using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class SelectWorkspaceTest : BaseTest
{
    private const string Url = "/dashboard/user/select-workspace";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserDao _userDao;

    public SelectWorkspaceTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SelectWorkspaceRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSelectWorkspace()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).First();

        var response = await PostRequestAsync(Url, _jwtToken, new SelectWorkspaceRequest
        {
            WorkspaceId = workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        await FlushDbChanges(isClearSession: true);
        var persistedUser = await _userDao.GetById(_user.Id);

        Assert.NotNull(actualUser.SelectedWorkspace);
        Assert.Equal(workspace.Id, actualUser.SelectedWorkspace.Id);
        Assert.Equal(workspace.Id, persistedUser!.SelectedWorkspace!.Id);
    }

    [Fact]
    public async Task ShouldReturnBadRequestIfUserHasNoAccess()
    {
        var otherUser = await UserSeeder.CreateActivatedAsync();
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(otherUser)).First();

        var response = await PostRequestAsync(Url, _jwtToken, new SelectWorkspaceRequest
        {
            WorkspaceId = workspace.Id
        });
        var responseData = await response.GetJsonResponseAsync<object>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("HasNoAccessException", responseData.ErrorCode);
    }
}
