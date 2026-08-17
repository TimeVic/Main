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
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetWorkspacePermissionsRequest()
        {
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OwnerShouldReceiveAllWorkspacePermissions()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetWorkspacePermissionsRequest()
        {
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        var expectedPermissions = Enum.GetValues<WorkspacePermission>();
        Assert.Equal(expectedPermissions.Length, actual.Permissions.Count);
        Assert.All(
            expectedPermissions,
            permission => Assert.Contains(permission, actual.Permissions)
        );
        Assert.Contains(WorkspacePermission.ReadUserPaymentReport, actual.Permissions);
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
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        var expectedPermissions = Enum.GetValues<WorkspacePermission>();
        Assert.Equal(expectedPermissions.Length, actual.Permissions.Count);
        Assert.All(
            expectedPermissions,
            permission => Assert.Contains(permission, actual.Permissions)
        );
        Assert.Contains(WorkspacePermission.ReadUserPaymentReport, actual.Permissions);
    }

    [Fact]
    public async Task UserShouldReceiveReadOnlyWorkspaceAndPaymentPermissions()
    {
        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        Assert.Equal(_workspace.Id, actual.WorkspaceId);
        Assert.Contains(WorkspacePermission.ReadWorkspaceSettings, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadWorkspaceMembers, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadClientPayment, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadMemberPayment, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadUserPaymentReport, actual.Permissions);
        Assert.Equal(5, actual.Permissions.Count);
        Assert.DoesNotContain(WorkspacePermission.UpdateWorkspaceMembers, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.CreateClientPayment, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.UpdateClientPayment, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.CreateMemberPayment, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.UpdateMemberPayment, actual.Permissions);
        Assert.DoesNotContain(WorkspacePermission.CreateMemberPaymentForOtherMembers, actual.Permissions);
    }

    [Fact]
    public async Task UserShouldNotReceiveWorkspacePermissionsWithWriteAccess()
    {
        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        var writeAccessPermissions = new[]
        {
            WorkspacePermission.UpdateWorkspace,
            WorkspacePermission.UpdateWorkspaceMembers,
            WorkspacePermission.CreateProject,
            WorkspacePermission.UpdateProject,
            WorkspacePermission.CreateClient,
            WorkspacePermission.UpdateClient,
            WorkspacePermission.CreateClientPayment,
            WorkspacePermission.UpdateClientPayment,
            WorkspacePermission.CreateMemberPayment,
            WorkspacePermission.UpdateMemberPayment,
            WorkspacePermission.ReadWorkspaceFinancialSummary,
            WorkspacePermission.CreateMemberPaymentForOtherMembers,
            WorkspacePermission.ReadTeamSummaryReport
        };

        Assert.All(
            writeAccessPermissions,
            permission => Assert.DoesNotContain(permission, actual.Permissions)
        );
    }

    [Fact]
    public async Task UserWithoutWorkspaceMemberShouldNotReceivePermissions()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
        }, _workspace.Id);

        var actual = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), actual.ErrorCode);
    }

    [Fact]
    public async Task OwnerShouldNotReceiveMemberPermissionsInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new GetWorkspacePermissionsRequest()
        {
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        var memberPermissions = new[]
        {
            WorkspacePermission.ReadWorkspaceMembers,
            WorkspacePermission.UpdateWorkspaceMembers,
            WorkspacePermission.ReadMemberPayment,
            WorkspacePermission.CreateMemberPayment,
            WorkspacePermission.UpdateMemberPayment,
            WorkspacePermission.CreateMemberPaymentForOtherMembers
        };
        Assert.All(
            memberPermissions,
            permission => Assert.DoesNotContain(permission, actual.Permissions)
        );
        Assert.DoesNotContain(WorkspacePermission.ReadTeamSummaryReport, actual.Permissions);
        Assert.Contains(WorkspacePermission.ReadUserPaymentReport, actual.Permissions);
    }

    [Fact]
    public async Task UserShouldNotReceiveMemberPermissionsInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();

        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetWorkspacePermissionsRequest()
        {
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetWorkspacePermissionsResponse>();
        var memberPermissions = new[]
        {
            WorkspacePermission.ReadWorkspaceMembers,
            WorkspacePermission.UpdateWorkspaceMembers,
            WorkspacePermission.ReadMemberPayment,
            WorkspacePermission.CreateMemberPayment,
            WorkspacePermission.UpdateMemberPayment,
            WorkspacePermission.CreateMemberPaymentForOtherMembers
        };
        Assert.All(
            memberPermissions,
            permission => Assert.DoesNotContain(permission, actual.Permissions)
        );
        Assert.Contains(WorkspacePermission.ReadUserPaymentReport, actual.Permissions);
    }
}
