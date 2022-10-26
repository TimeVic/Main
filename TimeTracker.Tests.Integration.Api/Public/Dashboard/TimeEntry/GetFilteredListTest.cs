using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

public class GetFilteredListTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/filtered-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserSeeder _userSeeder;

    public GetFilteredListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetFilteredListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_defaultWorkspace, _user)).First();
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 6, expectedProject)).First();
        expectedEntry.Description = "Fake desCript223ion 123";
        expectedEntry.IsBillable = true;
        expectedEntry.Date = DateTime.UtcNow.AddDays(-5);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1,
            Search = "cript223",
            ClientId = expectedProject.Client.Id,
            ProjectId = expectedProject.Id,
            IsBillable = true,
            DateFrom = DateTime.Now.AddDays(-6),
            DateTo = DateTime.Now,
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Equal(1, actualDto.TotalCount);
    }

    [Fact]
    public async Task ShouldReceiveListWithSharedAccess()
    {
        var projects = await _projectSeeder.CreateSeveralAsync(_defaultWorkspace, _user, 2);
        var expectedProject = projects.First();
        var expectedProject2 = projects.Last();

        var (otherJwt, otherUser) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            projects: new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject }
            }
        );
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, otherUser, 3, expectedProject);
        
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 3, expectedProject);
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 3, expectedProject2);

        var response = await PostRequestAsync(Url, otherJwt, new GetFilteredListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Equal(3 + 3, actualDto.TotalCount);
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.User.Id == _user.Id || item.User.Id == otherUser.Id);
        });
    }
}
