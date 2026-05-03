using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Security;

public class GetWorkspacePermissionsTest: BaseTest
{
    private readonly string Url = $"/{ApiUrl.WorkspacePermissions}";

    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;

    public GetWorkspacePermissionsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetWorkspacePermissionsRequest()
        {
            WorkspaceId = _workspace.Id
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OwnerShouldReceiveAllWorkspacePermissions()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetWorkspacePermissionsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        Assert.Equal(Enum.GetValues<WorkspacePermission>().Length, actual.Permissions.Count);
        Assert.All(
            Enum.GetValues<WorkspacePermission>(),
            permission => Assert.Contains(permission, actual.Permissions)
        );
    }

    [Fact]
    public async Task ManagerShouldReceiveAllWorkspacePermissions()
    {
        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        Assert.Equal(Enum.GetValues<WorkspacePermission>().Length, actual.Permissions.Count);
        Assert.All(
            Enum.GetValues<WorkspacePermission>(),
            permission => Assert.Contains(permission, actual.Permissions)
        );
    }

    [Fact]
    public async Task UserShouldReceiveReadWorkspaceSettingsAndReadWorkspaceMembers()
    {
        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        Assert.Equal(2, actual.Permissions.Count);
        Assert.Contains(WorkspacePermission.ReadWorkspaceSettings, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadWorkspaceMembers, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.UpdateWorkspaceMembers, actual.Permissions);
    }

    [Fact]
    public async Task UserWithoutWorkspaceMemberShouldNotReceivePermissions()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
            WorkspaceId = _workspace.Id
        });

        var actual = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), actual.ErrorCode);
    }
}
