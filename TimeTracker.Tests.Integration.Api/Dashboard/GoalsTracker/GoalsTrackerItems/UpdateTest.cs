using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.GoalsTracker.GoalsTrackerItems;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/goals-tracker/item/update";
    
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

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _goalsTrackerDao = ServiceProvider.GetRequiredService<IGoalsTrackerDao>();
        _goalsTrackerSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerSeeder>();
        _goalsTrackerItemsSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerItemsSeeder>();
        _factory = ServiceProvider.GetRequiredService<IDataFactory<GoalsTrackerItemEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _tracker = _goalsTrackerSeeder.CreateAsync(_user, _workspace).Result;
        _trackerItem = _goalsTrackerItemsSeeder.CreateAsync(_tracker).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectedItem = _factory.Generate();
        
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateItemRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            Name = expectedItem.Name,
            NumberOfTimes = expectedItem.NumberOfTimes
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        // Arrange
        var expectedItem = _factory.Generate();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateItemRequest()
        {
            GoalsTrackerItemId = _trackerItem.Id,
            Name = expectedItem.Name,
            NumberOfTimes = expectedItem.NumberOfTimes
        });
        response.EnsureSuccessStatusCode();

        // Assert
        var actualItem = await response.GetJsonDataAsync<GoalsTrackerItemDto>();
        Assert.NotEqual(Guid.Empty, actualItem.Id);
        Assert.Equal(expectedItem.Name, actualItem.Name);
        Assert.Equal(expectedItem.NumberOfTimes, actualItem.NumberOfTimes);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfIncorrectTrackerId()
    {
        // Arrange
        var expectedItem = _factory.Generate();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateItemRequest()
        {
            GoalsTrackerItemId = Guid.Empty,
            Name = expectedItem.Name,
            NumberOfTimes = expectedItem.NumberOfTimes
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.Type);
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
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateItemRequest()
        {
            GoalsTrackerItemId = otherTrackerItem.Id,
            Name = expectedItem.Name,
            NumberOfTimes = expectedItem.NumberOfTimes
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
