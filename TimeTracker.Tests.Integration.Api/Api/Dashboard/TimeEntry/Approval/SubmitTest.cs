using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class SubmitTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/submit";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public SubmitTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();

        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SubmitRequest { TimeEntryId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSubmitEntryForApproval()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(1);
        entry.Status = TimeEntryStatus.Draft;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new SubmitRequest
        {
            TimeEntryId = entry.Id
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotNull(actual);
        Assert.Equal(entry.Id, actual.Id);
        Assert.Equal(TimeEntryStatus.Pending, actual.Status);
    }
}
