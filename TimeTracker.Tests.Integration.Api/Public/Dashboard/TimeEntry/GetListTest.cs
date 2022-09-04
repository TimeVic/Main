using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _timeEntrySeeder.CreateSeveralAsync(_user, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<PaginatedListDto<TimeEntryDto>>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotNull(item.Project);
            Assert.NotEmpty(item.Description);
            Assert.True(item.StartTime > DateTime.MinValue);
            Assert.True(item.EndTime > DateTime.MinValue);
        });
    }
}
