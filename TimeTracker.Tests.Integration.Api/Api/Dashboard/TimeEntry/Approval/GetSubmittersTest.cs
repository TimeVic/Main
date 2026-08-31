using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry.Approval;

public class GetSubmittersTest : BaseTest
{
    private readonly string Url = "/dashboard/time-entry/approval/submitters";

    private readonly UserEntity _owner;
    private readonly UserEntity _developer;
    private readonly string _ownerJwtToken;
    private readonly string _developerJwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetSubmittersTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();

        (_ownerJwtToken, _owner, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        (_developerJwtToken, _developer, _) = UserSeeder.CreateAuthorizedAsync().Result;

        _defaultWorkspace.Mode = WorkspaceMode.Team;
        FlushDbChanges().Wait();

        _workspaceAccessService.ShareAccessAsync(_defaultWorkspace, _developer, MembershipAccessType.User).Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetSubmittersRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeveloperCannotGetSubmitters()
    {
        var response = await PostRequestAsync(Url, _developerJwtToken, new GetSubmittersRequest(), _defaultWorkspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OwnerCanGetSubmitters()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(4);
        entry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetSubmittersRequest(), _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetSubmittersResponse>();
        Assert.NotNull(actual);
        Assert.NotEmpty(actual.Items);

        var submitter = actual.Items.First(i => i.UserId == _developer.Id);
        Assert.Equal(_developer.Login, submitter.Login);
        Assert.Equal(1, submitter.PendingCount);
        Assert.Equal(TimeEntryStatus.Pending, submitter.Status);
    }

    [Fact]
    public async Task OwnerHoursAreExcludedFromSubmittersList()
    {
        var ownerEntry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _owner, 1)).First();
        ownerEntry.EndTime = ownerEntry.StartTime.AddHours(5);
        ownerEntry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetSubmittersRequest(), _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetSubmittersResponse>();
        Assert.NotNull(actual);
        Assert.DoesNotContain(actual.Items, i => i.UserId == _owner.Id);
    }

    [Fact]
    public async Task DeveloperWithOnlyDraftEntriesIsNotInSubmittersList()
    {
        var devEntry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _developer, 1)).First();
        devEntry.EndTime = devEntry.StartTime.AddHours(5);
        devEntry.Status = TimeEntryStatus.Draft;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _ownerJwtToken, new GetSubmittersRequest(), _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<GetSubmittersResponse>();
        Assert.NotNull(actual);
        Assert.DoesNotContain(actual.Items, i => i.UserId == _developer.Id);
    }
}
