using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class RejectTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/reject";

    private readonly UserEntity _owner;
    private readonly UserEntity _developer;
    private readonly string _ownerJwtToken;
    private readonly string _developerJwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public RejectTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();

        (_ownerJwtToken, _owner, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        (_developerJwtToken, _developer, _) = UserSeeder.CreateAuthorizedAsync().Result;

        _defaultWorkspace.Mode = WorkspaceMode.Team;
        _defaultWorkspace.IsApprovalsEnabled = true;
        FlushDbChanges().Wait();

        _workspaceAccessService.ShareAccessAsync(_defaultWorkspace, _developer, MembershipAccessType.User).Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new RejectRequest
        {
            TimeEntryIds = new List<Guid> { Guid.NewGuid() },
            Reason = "Test reason"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectEntriesWithReason()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new RejectRequest
        {
            TimeEntryIds = new List<Guid> { entry.Id },
            Reason = "Incorrect description"
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<PaginatedListDto<TimeEntryDto>>();
        Assert.NotNull(actual);
        Assert.Equal(1, actual.TotalCount);
        Assert.Equal(TimeEntryStatus.Rejected, actual.Items.First().Status);
        Assert.Equal("Incorrect description", actual.Items.First().LatestRejectComment);
    }
}
