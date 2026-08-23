using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Counters;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard;

public class GetCountersTest : BaseTest
{
    private const string Url = "/dashboard/counters";

    private readonly UserEntity _owner;
    private readonly string _ownerJwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetCountersTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_ownerJwtToken, _owner, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetCountersRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OwnerCanGetPendingApprovalsCount()
    {
        _workspace.Mode = WorkspaceMode.Team;
        _workspace.IsApprovalsEnabled = true;

        var member1 = await _userSeeder.CreateActivatedAsync();
        var member2 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, member1, MembershipAccessType.User);
        await _workspaceAccessService.ShareAccessAsync(_workspace, member2, MembershipAccessType.User);

        // member1 has 2 pending entries
        var entry1 = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, member1, 1)).First();
        entry1.EndTime = entry1.StartTime.AddHours(2);
        entry1.Status = TimeEntryStatus.Pending;

        var entry2 = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, member1, 1)).First();
        entry2.EndTime = entry2.StartTime.AddHours(3);
        entry2.Status = TimeEntryStatus.Pending;

        // member2 has 1 pending entry
        var entry3 = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, member2, 1)).First();
        entry3.EndTime = entry3.StartTime.AddHours(4);
        entry3.Status = TimeEntryStatus.Pending;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetCountersRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetCountersResponse>();
        Assert.NotNull(actual);
        Assert.NotNull(actual.Counters);
        // 2 distinct users with pending entries
        Assert.Equal(2, actual.Counters.PendingApprovalsCount);
    }

    [Fact]
    public async Task OwnerOwnEntriesExcludedFromPendingApprovalsCount()
    {
        _workspace.Mode = WorkspaceMode.Team;
        _workspace.IsApprovalsEnabled = true;

        var ownerEntry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _owner, 1)).First();
        ownerEntry.EndTime = ownerEntry.StartTime.AddHours(2);
        ownerEntry.Status = TimeEntryStatus.Pending;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetCountersRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetCountersResponse>();
        Assert.NotNull(actual);
        Assert.Equal(0, actual.Counters.PendingApprovalsCount);
    }

    [Fact]
    public async Task RegularMemberGetsZeroPendingApprovalsCount()
    {
        _workspace.Mode = WorkspaceMode.Team;
        _workspace.IsApprovalsEnabled = true;

        var (memberJwtToken, member, _) = await _userSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, member, MembershipAccessType.User);

        var member2 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, member2, MembershipAccessType.User);
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, member2, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Pending;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, memberJwtToken, new GetCountersRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetCountersResponse>();
        Assert.NotNull(actual);
        Assert.Equal(0, actual.Counters.PendingApprovalsCount);
    }
}
