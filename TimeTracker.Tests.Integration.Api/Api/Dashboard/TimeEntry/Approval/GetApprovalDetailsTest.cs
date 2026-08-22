using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class GetApprovalDetailsTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/details";

    private readonly UserEntity _owner;
    private readonly UserEntity _developer;
    private readonly string _ownerJwtToken;
    private readonly string _developerJwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetApprovalDetailsTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var startOfWeek = DateTime.UtcNow.StartOfWeek();
        var endOfWeek = startOfWeek.AddDays(6).EndOfDay();
        var response = await PostRequestAsAnonymousAsync(Url, new GetApprovalDetailsRequest
        {
            UserId = _developer.Id,
            StartDate = startOfWeek,
            EndDate = endOfWeek
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeveloperCannotGetDetails()
    {
        var startOfWeek = DateTime.UtcNow.StartOfWeek();
        var endOfWeek = startOfWeek.AddDays(6).EndOfDay();
        var response = await PostRequestAsync(Url, _developerJwtToken, new GetApprovalDetailsRequest
        {
            UserId = _developer.Id,
            StartDate = startOfWeek,
            EndDate = endOfWeek
        }, _defaultWorkspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OwnerCanGetDetails()
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.StartOfWeek();
        var endOfWeek = startOfWeek.AddDays(6).EndOfDay();

        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _developer, 1)).First();
        entry.StartTime = startOfWeek.AddDays(1).AddHours(9);
        entry.EndTime = entry.StartTime.AddHours(3);
        entry.Status = TimeEntryStatus.Pending;
        entry.HourlyRate = 50m;
        entry.IsBillable = true;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetApprovalDetailsRequest
        {
            UserId = _developer.Id,
            StartDate = startOfWeek,
            EndDate = endOfWeek
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetApprovalDetailsResponse>();
        Assert.NotNull(actual);
        Assert.Equal(_developer.Id, actual.UserId);
        Assert.Equal(_developer.Name, actual.UserName);
        Assert.Equal(TimeSpan.FromHours(3), actual.TotalDuration);
        Assert.NotEmpty(actual.Projects);

        var firstProject = actual.Projects.First();
        Assert.NotEmpty(firstProject.Tasks);

        var firstTask = firstProject.Tasks.First();
        Assert.NotEmpty(firstTask.Entries);
        Assert.Equal(entry.Id, firstTask.Entries.First().Id);
    }
}
