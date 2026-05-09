using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.GoalsTracker;

public class ChangePositionsTest: BaseTest
{
    private readonly string Url = "/dashboard/goals-tracker/change-positions";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private new readonly IDataFactory<ClientEntity> _factory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly IGoalsTrackerDao _goalsTrackerDao;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;
    private readonly IGoalsTrackerItemsSeeder _goalsTrackerItemsSeeder;
    private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

    public ChangePositionsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _goalsTrackerDao = ServiceProvider.GetRequiredService<IGoalsTrackerDao>();
        _goalsTrackerItemsDao = ServiceProvider.GetRequiredService<IGoalsTrackerItemsDao>();
        _goalsTrackerSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerSeeder>();
        _goalsTrackerItemsSeeder = ServiceProvider.GetRequiredService<IGoalsTrackerItemsSeeder>();
        _factory = ServiceProvider.GetRequiredService<IDataFactory<ClientEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var client = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new ChangePositionsRequest()
        {
            Date = DateTime.Now,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGetExistsWithItems()
    {
        // Arrange
        var expectedDate = DateTime.Now.AddMonths(-3);
        
        var existsTracker = await _goalsTrackerSeeder.CreateAsync(_user, _workspace);
        existsTracker.Year = expectedDate.Year;
        existsTracker.Month = expectedDate.Month;
        var goals = await _goalsTrackerItemsSeeder.CreateSeveralAsync(existsTracker, 4);

        var goal1 = goals.First();
        var goal2 = goals.Skip(1).First();
        var goal3 = goals.Skip(2).First();
        var goal4 = goals.Skip(3).First();
        
        await FlushDbChanges();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new ChangePositionsRequest()
        {
            Date = expectedDate,
            Positions = new Dictionary<Guid, int>()
            {
                { goal1.Id, 6 },
                { goal2.Id, 2 },
                { goal3.Id, 4 },
                { goal4.Id, 1 },
            }
        });
        
        // Assert
        response.EnsureSuccessStatusCode();

        await FlushDbChanges(true);
        await DbSessionProvider.CurrentSession.RefreshAsync(existsTracker);
        Assert.Equal(6, existsTracker.Items.First(item => item.Id == goal1.Id).Position);
        Assert.Equal(2, existsTracker.Items.First(item => item.Id == goal2.Id).Position);
        Assert.Equal(4, existsTracker.Items.First(item => item.Id == goal3.Id).Position);
        Assert.Equal(1, existsTracker.Items.First(item => item.Id == goal4.Id).Position);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var user2 = await UserSeeder.CreateActivatedAsync();
        var otherWorkspace = (await _userDao.GetUsersWorkspaces(user2, MembershipAccessType.Owner)).First();
        var response = await PostRequestAsync(Url, _jwtToken, new ChangePositionsRequest()
        {
            Date = DateTime.Now
        }, otherWorkspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.ErrorCode);
    }
}
