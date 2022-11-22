using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IUserSeeder _userSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
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
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.List.TotalCount);
        
        Assert.All(actualDto.List.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotNull(item.Project);
            Assert.NotEmpty(item.Description);
            Assert.True(item.StartTime > TimeSpan.MinValue);
            Assert.True(item.EndTime > TimeSpan.MinValue);
            Assert.True(item.Date > DateTime.MinValue);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUserList()
    {
        var expectedCounter = 15;
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, expectedCounter);
        await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow, 
            TimeSpan.FromSeconds(1)
        );
     
        var otherUser = await _userSeeder.CreateActivatedAndShareAsync(_defaultWorkspace);
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, otherUser, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter + 1, actualDto.List.TotalCount);

        var activeEntry = await _timeEntryDao.GetActiveEntryAsync(_defaultWorkspace, _user);
        Assert.Equal(activeEntry.Id, actualDto.ActiveTimeEntry.Id);
    }
    
    [Fact]
    public async Task ShouldReceiveListWithTimeActiveTimeEntry()
    {
        var expectedCounter = 15;
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, expectedCounter);
        await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow, 
            TimeSpan.FromSeconds(1)
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.NotNull(actualDto.ActiveTimeEntry);
        Assert.True(actualDto.ActiveTimeEntry.Id > 0);
    }
}
