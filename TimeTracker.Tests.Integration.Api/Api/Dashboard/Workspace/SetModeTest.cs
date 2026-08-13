using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Workspace;

public class SetModeTest : BaseTest
{
    private readonly string Url = "/dashboard/workspace/set-mode";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;

    public SetModeTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSetModeToSolo()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.Equal(_workspace.Id, actual.Id);
        Assert.Equal(WorkspaceMode.Solo, actual.Mode);
    }

    [Fact]
    public async Task ShouldSetModeToTeam()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Team
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.Equal(_workspace.Id, actual.Id);
        Assert.Equal(WorkspaceMode.Team, actual.Mode);
    }

    [Fact]
    public async Task ShouldNotSetModeIfAlreadySet()
    {
        // First set mode to Solo
        var response1 = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        });
        response1.EnsureSuccessStatusCode();

        // Attempt to set mode again to Team
        var response2 = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Team
        });

        var actual = await response2.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), actual.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotSetModeUserWithRoleUser()
    {
        var (otherJwt, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwt, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        }, _workspace.Id);

        var actual = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), actual.ErrorCode);
    }

    [Fact]
    public async Task ShouldSetModeForAdditionalUserCreatedWorkspace()
    {
        var createdWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        Assert.Null(createdWorkspace.Mode);

        // Set mode on the newly created additional workspace
        var setModeResponse = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        }, createdWorkspace.Id);
        setModeResponse.EnsureSuccessStatusCode();

        var updatedWorkspace = await setModeResponse.GetJsonDataAsync<WorkspaceDto>();
        Assert.Equal(createdWorkspace.Id, updatedWorkspace.Id);
        Assert.Equal(WorkspaceMode.Solo, updatedWorkspace.Mode);
    }

    [Fact]
    public async Task ShouldNotSetModeForAdditionalWorkspaceIfAlreadySet()
    {
        var createdWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        Assert.Null(createdWorkspace.Mode);

        // First set mode to Team
        var setModeResponse1 = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Team
        }, createdWorkspace.Id);
        setModeResponse1.EnsureSuccessStatusCode();

        // Attempt to set mode on the additional workspace again
        var setModeResponse2 = await PostRequestAsync(Url, _jwtToken, new SetModeRequest
        {
            Mode = WorkspaceMode.Solo
        }, createdWorkspace.Id);

        var actual = await setModeResponse2.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), actual.ErrorCode);
    }
}
