using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class SubmitPeriodTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/submit-period";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public SubmitPeriodTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();

        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
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
    public async Task ShouldSubmitPeriodDraftEntries()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 2)).ToList();
        var baseDate = DateTime.UtcNow.Date;
        entries[0].StartTime = baseDate.AddDays(-2);
        entries[0].EndTime = entries[0].StartTime.AddHours(2);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].StartTime = baseDate.AddDays(-1);
        entries[1].EndTime = entries[1].StartTime.AddHours(2);
        entries[1].Status = TimeEntryStatus.Draft;

        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new SubmitPeriodRequest
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
}
