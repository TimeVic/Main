using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectSeeder _projectSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
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
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(1, actualDto.List.TotalCount);
        
        Assert.All(actualDto.List.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.User.Id);
            Assert.NotNull(item.Project);
            Assert.NotEmpty(item.Description!);
            Assert.True(item.StartTime > DateTime.MinValue);
            Assert.True(item.EndTime > DateTime.MinValue);
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
            DateTime.UtcNow.AddSeconds(1)
        );
     
        var otherUser = await _userSeeder.CreateActivatedAndShareAsync(_defaultWorkspace);
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, otherUser, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(1, actualDto.List.TotalCount);

        var activeEntry = await _timeEntryDao.GetActiveEntryAsync(_defaultWorkspace, _user);
        Assert.NotNull(actualDto.ActiveTimeEntry);
        Assert.NotNull(activeEntry);
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
            DateTime.UtcNow.AddSeconds(1)
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.NotNull(actualDto.ActiveTimeEntry);
        Assert.NotEqual(Guid.Empty, actualDto.ActiveTimeEntry.Id);
    }

    [Fact]
    public async Task ShouldNotSplitSingleDayBetweenPages()
    {
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        var daysInPage = GlobalConstants.TimeEntryGroupedByDayPageSize;
        var baseDay = DateTime.UtcNow.Date;
        var boundaryDay = baseDay.AddDays(-(daysInPage - 1));

        for (var i = 0; i < daysInPage + 2; i++)
        {
            var startTime = baseDay.AddDays(-i).AddHours(10);
            await _timeEntryDao.SetAsync(
                _user,
                _defaultWorkspace,
                new TimeEntryCreationDto
                {
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1),
                    IsBillable = true,
                    HourlyRate = 10
                },
                project
            );
        }

        for (var i = 0; i < 3; i++)
        {
            var startTime = boundaryDay.AddHours(12 + i);
            await _timeEntryDao.SetAsync(
                _user,
                _defaultWorkspace,
                new TimeEntryCreationDto
                {
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(30),
                    IsBillable = true,
                    HourlyRate = 12
                },
                project
            );
        }

        var firstPageResponse = await PostRequestAsync(Url, _jwtToken, new GetListRequest
        {
            Page = 1
        });
        firstPageResponse.EnsureSuccessStatusCode();
        var firstPage = await firstPageResponse.GetJsonDataAsync<GetListResponse>();

        var secondPageResponse = await PostRequestAsync(Url, _jwtToken, new GetListRequest
        {
            Page = 2
        });
        secondPageResponse.EnsureSuccessStatusCode();
        var secondPage = await secondPageResponse.GetJsonDataAsync<GetListResponse>();

        var boundaryItemsOnFirstPage = firstPage.List.Items
            .Where(item => item.StartTime.Date == boundaryDay)
            .ToList();
        var boundaryItemsOnSecondPage = secondPage.List.Items
            .Where(item => item.StartTime.Date == boundaryDay)
            .ToList();

        Assert.True(boundaryItemsOnFirstPage.Count >= 4);
        Assert.Empty(boundaryItemsOnSecondPage);
    }

}
