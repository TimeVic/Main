using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class GetStatusTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/status";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public GetStatusTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();

        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace.Mode = WorkspaceMode.Team;
        _defaultWorkspace.IsApprovalsEnabled = true;
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetStatusRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnStatusSummary()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 2)).ToList();
        entries[0].HourlyRate = 20;
        entries[0].IsBillable = true;
        entries[0].StartTime = DateTime.UtcNow.AddHours(-3);
        entries[0].EndTime = entries[0].StartTime.AddHours(2);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].HourlyRate = 20;
        entries[1].IsBillable = true;
        entries[1].StartTime = DateTime.UtcNow.AddHours(-1);
        entries[1].EndTime = entries[1].StartTime.AddHours(1);
        entries[1].Status = TimeEntryStatus.Pending;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new GetStatusRequest(), _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<TimeEntryApprovalStatusSummaryDto>();
        Assert.NotNull(actual);
        Assert.Equal(1, actual.DraftCount);
        Assert.Equal(40m, actual.DraftAmount);
        Assert.Equal(1, actual.PendingCount);
        Assert.Equal(20m, actual.PendingAmount);
        Assert.Equal(60m, actual.PendingAndDraftAmount);
    }
}
