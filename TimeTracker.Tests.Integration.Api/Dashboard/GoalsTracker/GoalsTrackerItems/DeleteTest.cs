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

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/goals-tracker/item/delete";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;
    private readonly IGoalsTrackerItemsSeeder _goalsTrackerItemsSeeder;
    private readonly GoalsTrackerEntity _tracker;
    private readonly GoalsTrackerItemEntity _trackerItem;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _goalsTrackerSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerSeeder>();
        _goalsTrackerItemsSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerItemsSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _tracker = _goalsTrackerSeeder.CreateAsync(_user, _workspace).Result;
        _trackerItem = _goalsTrackerItemsSeeder.CreateAsync(_tracker).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteItemRequest()
        {
            Id = _trackerItem.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteItemRequest()
        {
            Id = _trackerItem.Id
        });
        
        // Assert
        await response.EnsureSuccessStatusCodeWithoutError();

        await FlushDbChanges(true);
        var actualItem = await DbSessionProvider.CurrentSession.GetAsync<GoalsTrackerItemEntity>(_trackerItem.Id);
        Assert.True(actualItem.IsArchived);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfIncorrectTrackerId()
    {
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteItemRequest()
        {
            Id = Guid.Empty
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
        var user2 = await UserSeeder.CreateActivatedAsync();
        var otherTracker = await _goalsTrackerSeeder.CreateAsync(user2, user2.CreatedWorkspaces.First());
        var otherTrackerItem = await _goalsTrackerItemsSeeder.CreateAsync(otherTracker);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteItemRequest()
        {
            Id = otherTrackerItem.Id
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
