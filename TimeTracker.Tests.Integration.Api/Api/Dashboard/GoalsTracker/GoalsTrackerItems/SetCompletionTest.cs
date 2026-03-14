using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.GoalsTracker.GoalsTrackerItems;

public class SetCompletionTest: BaseTest
{
    private readonly string Url = "/dashboard/goals-tracker/item/set-completion";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<GoalsTrackerItemEntity> _factory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly IGoalsTrackerDao _goalsTrackerDao;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;
    private readonly IGoalsTrackerItemsSeeder _goalsTrackerItemsSeeder;
    private readonly GoalsTrackerEntity _tracker;
    private readonly GoalsTrackerItemEntity _trackerItem;
    private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

    public SetCompletionTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _goalsTrackerDao = ServiceProvider.GetRequiredService<IGoalsTrackerDao>();
        _goalsTrackerSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerSeeder>();
        _goalsTrackerItemsSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerItemsSeeder>();
        _goalsTrackerItemsDao = ServiceProvider.GetRequiredService<IGoalsTrackerItemsDao>();
        _factory = ServiceProvider.GetRequiredService<IDataFactory<GoalsTrackerItemEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _tracker = _goalsTrackerSeeder.CreateAsync(_user, _workspace).Result;
        _trackerItem = _goalsTrackerItemsSeeder.CreateAsync(_tracker).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SetCompletionRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            DayOfMonth = 1,
            IsChecked = true
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldSet()
    {
        // Arrange
        var expectedDayOfMonth = DateTime.DaysInMonth(_tracker.Year, _tracker.Month);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            DayOfMonth = expectedDayOfMonth,
            IsChecked = true
        });
        response.EnsureSuccessStatusCode();

        // Assert
        var actualMarker = await response.GetJsonDataAsync<GoalsTrackerCompletionMarkerDto>();
        Assert.NotEqual(Guid.Empty, actualMarker.Id);
        Assert.Equal(expectedDayOfMonth, actualMarker.DayOfMonth);
        Assert.True(actualMarker.IsChecked);

        await FlushDbChanges(true);
        var actualItem = await DbSessionProvider.CurrentSession.GetAsync<GoalsTrackerItemEntity>(_trackerItem.Id);
        Assert.Single(actualItem.CompletionMarkers);
    }
    
    [Fact]
    public async Task ShouldAddNewIfPreviouslySet()
    {
        // Arrange
        var expectedDayOfMonth = DateTime.DaysInMonth(_tracker.Year, _tracker.Month);
        await _goalsTrackerItemsDao.SetCompletion(_trackerItem, expectedDayOfMonth - 1, true);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            DayOfMonth = expectedDayOfMonth,
            IsChecked = true
        });
        response.EnsureSuccessStatusCode();

        // Assert
        var actualMarker = await response.GetJsonDataAsync<GoalsTrackerCompletionMarkerDto>();
        Assert.NotEqual(Guid.Empty, actualMarker.Id);
        Assert.Equal(expectedDayOfMonth, actualMarker.DayOfMonth);
        Assert.True(actualMarker.IsChecked);

        await FlushDbChanges(true);
        var actualItem = await DbSessionProvider.CurrentSession.GetAsync<GoalsTrackerItemEntity>(_trackerItem.Id);
        Assert.Equal(2, actualItem.CompletionMarkers.Count);
    }
    
    [Fact]
    public async Task ShouldUpdateExists()
    {
        // Arrange
        var expectedDayOfMonth = DateTime.DaysInMonth(_tracker.Year, _tracker.Month) - 2;
        
        // Act
        await _goalsTrackerItemsDao.SetCompletion(_trackerItem, expectedDayOfMonth, true);
        var actualItem = await DbSessionProvider.CurrentSession.GetAsync<GoalsTrackerItemEntity>(_trackerItem.Id);
        Assert.Single(actualItem.CompletionMarkers);
        Assert.True(actualItem.CompletionMarkers.First().IsChecked);
        
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            DayOfMonth = expectedDayOfMonth,
            IsChecked = false
        });
        response.EnsureSuccessStatusCode();
        
        // Assert
        var actualMarker = await response.GetJsonDataAsync<GoalsTrackerCompletionMarkerDto>();
        Assert.NotEqual(Guid.Empty, actualMarker.Id);
        Assert.Equal(expectedDayOfMonth, actualMarker.DayOfMonth);
        Assert.False(actualMarker.IsChecked);

        await FlushDbChanges(true);
        actualItem = await DbSessionProvider.CurrentSession.GetAsync<GoalsTrackerItemEntity>(_trackerItem.Id);
        Assert.Single(actualItem.CompletionMarkers);
        Assert.False(actualItem.CompletionMarkers.First().IsChecked);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfIncorrectTrackerId()
    {
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = Guid.Empty,
            DayOfMonth = 2,
            IsChecked = true
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldNotUpdateForOtherTracker()
    {
        // Arrange
        var expectedItem = _factory.Generate();
        var user2 = await UserSeeder.CreateActivatedAsync();
        var otherTracker = await _goalsTrackerSeeder.CreateAsync(user2, user2.CreatedWorkspaces.First());
        var otherTrackerItem = await _goalsTrackerItemsSeeder.CreateAsync(otherTracker);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = otherTrackerItem.Id,
            DayOfMonth = 2,
            IsChecked = true
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task DayOfMonthCanNotBeMoreThanDaysInMonth()
    {
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new SetCompletionRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            DayOfMonth = DateTime.DaysInMonth(_tracker.Year, _tracker.Month) + 1,
            IsChecked = true
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new DataValidationException().GetTypeName(), error.ErrorCode);
    }
}
