using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class SubmitPeriodTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/submit-period";

    private readonly UserEntity _owner;
    private readonly UserEntity _developer;
    private readonly string _ownerJwtToken;
    private readonly string _developerJwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SubmitPeriodTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var response = await PostRequestAsAnonymousAsync(Url, new SubmitPeriodRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeveloperSubmitPeriodTransitionsToPending()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _developer, 2)).ToList();
        var baseDate = DateTime.UtcNow.Date;
        entries[0].StartTime = baseDate.AddDays(-2);
        entries[0].EndTime = entries[0].StartTime.AddHours(2);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].StartTime = baseDate.AddDays(-1);
        entries[1].EndTime = entries[1].StartTime.AddHours(2);
        entries[1].Status = TimeEntryStatus.Draft;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _developerJwtToken, new SubmitPeriodRequest
        {
            StartDate = baseDate.AddDays(-5),
            EndDate = baseDate
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<PaginatedListDto<TimeEntryDto>>();
        Assert.NotNull(actual);
        Assert.Equal(2, actual.TotalCount);
        Assert.All(actual.Items, e => Assert.Equal(TimeEntryStatus.Pending, e.Status));
    }

    [Fact]
    public async Task OwnerSubmitPeriodDirectlyApproves()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _owner, 2)).ToList();
        var baseDate = DateTime.UtcNow.Date;
        entries[0].StartTime = baseDate.AddDays(-2);
        entries[0].EndTime = entries[0].StartTime.AddHours(2);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].StartTime = baseDate.AddDays(-1);
        entries[1].EndTime = entries[1].StartTime.AddHours(2);
        entries[1].Status = TimeEntryStatus.Draft;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new SubmitPeriodRequest
        {
            StartDate = baseDate.AddDays(-5),
            EndDate = baseDate
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<PaginatedListDto<TimeEntryDto>>();
        Assert.NotNull(actual);
        Assert.Equal(2, actual.TotalCount);
        Assert.All(actual.Items, e => Assert.Equal(TimeEntryStatus.Approved, e.Status));
    }
}
