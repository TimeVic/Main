using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard;

public class InitTest : BaseTest
{
    private const string Url = "/dashboard/init";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IClientSeeder _clientSeeder;
    private readonly ITagSeeder _tagSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public InitTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DashboardInitRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReceiveInitData()
    {
        _workspace.Mode = WorkspaceMode.Team;
        _workspace.IsApprovalsEnabled = true;

        var clients = await _clientSeeder.CreateSeveralAsync(_workspace, 2);
        var project1 = await _projectSeeder.CreateAsync(_workspace, clients.First());
        var project2 = await _projectSeeder.CreateAsync(_workspace, clients.Last());
        var projects = new[] { project1, project2 };
        await _tagSeeder.CreateSeveralAsync(_workspace, 2);
        await _taskListSeeder.CreateSeveralAsync(projects.First(), 2);
        var activeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 1, projects.First())).First();
        activeEntry.EndTime = null;

        // Create a team member with pending time entry
        var member = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, member, MembershipAccessType.User);
        var memberEntry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, member, 1, projects.Last())).First();
        memberEntry.EndTime = memberEntry.StartTime.AddHours(2);
        memberEntry.Status = TimeEntryStatus.Pending;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new DashboardInitRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<DashboardInitResponse>();
        Assert.NotNull(actual);

        // Current user
        Assert.Equal(_user.Id, actual.CurrentUser.Id);
        Assert.Equal(_user.Email, actual.CurrentUser.Email);

        // Workspaces & Current Workspace
        Assert.NotEmpty(actual.Workspaces);
        Assert.Equal(_workspace.Id, actual.CurrentWorkspace.Id);
        Assert.Equal(MembershipAccessType.Owner, actual.CurrentWorkspace.CurrentUserAccess);

        // Permissions
        Assert.NotEmpty(actual.Permissions);
        Assert.Contains(WorkspacePermission.UpdateWorkspace, actual.Permissions);

        // Projects, Clients, Tags, TaskLists
        Assert.Equal(2, actual.Projects.Count);
        Assert.Equal(2, actual.Clients.Count);
        Assert.Equal(2, actual.Tags.Count);
        Assert.Equal(2, actual.TaskLists.Count);

        // Active Time Entry
        Assert.NotNull(actual.ActiveTimeEntry);
        Assert.Equal(activeEntry.Id, actual.ActiveTimeEntry.Id);

        // Workspace Members
        Assert.Contains(actual.WorkspaceMembers, m => m.User.Id == member.Id);
    }

    [Fact]
    public async Task ShouldReceiveInitDataForSpecificWorkspace()
    {
        var otherWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user, 1)).First();
        await _projectSeeder.CreateSeveralAsync(otherWorkspace, 3);
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new DashboardInitRequest(), otherWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<DashboardInitResponse>();
        Assert.NotNull(actual);
        Assert.Equal(otherWorkspace.Id, actual.CurrentWorkspace.Id);
        Assert.Equal(3, actual.Projects.Count);
    }
}
