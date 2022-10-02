using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
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

    public GetFilteredListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
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
        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_user)).First();
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_user, 6, expectedProject)).First();
        expectedEntry.Description = "Fake desCript223ion 123";
        expectedEntry.IsBillable = true;
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1,
            Search = "cript223",
            ClientId = expectedProject.Client.Id,
            ProjectId = expectedProject.Id,
            IsBillable = true
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Equal(1, actualDto.TotalCount);
    }
}
