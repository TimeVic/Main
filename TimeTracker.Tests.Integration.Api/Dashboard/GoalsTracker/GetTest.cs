using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.GoalsTracker;

public class GetTest: BaseTest
{
    private readonly string Url = "/dashboard/goals-tracker/get";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<ClientEntity> _factory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly IGoalsTrackerDao _goalsTrackerDao;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;
    private readonly IGoalsTrackerItemsSeeder _goalsTrackerItemsSeeder;
    private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

    public GetTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var response = await PostRequestAsAnonymousAsync(Url, new GetRequest()
        {
            Date = DateTime.Now,
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGetEmpty()
    {
        var expectedDate = DateTime.Now;
        var response = await PostRequestAsync(Url, _jwtToken, new GetRequest()
        {
            Date = DateTime.Now,
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<GoalsTrackerDto>();
        Assert.NotEqual(Guid.Empty, actualProject.Id);
        Assert.Equal(expectedDate.Year, actualProject.Year);
        Assert.Equal(expectedDate.Month, actualProject.Month);
        Assert.Empty(actualProject.Items);
        Assert.Empty(actualProject.Notes);
    }
    
    [Fact]
    public async Task ShouldGetExistsWithItems()
    {
        var expectedDate = DateTime.Now.AddMonths(-3);
        
        var existsTracker = await _goalsTrackerSeeder.CreateAsync(_user, _workspace);
        existsTracker.Year = expectedDate.Year;
        existsTracker.Month = expectedDate.Month;
        await _goalsTrackerItemsSeeder.CreateSeveralAsync(existsTracker, 4);
        await CommitDbChanges();
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetRequest()
        {
            Date = expectedDate,
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<GoalsTrackerDto>();
        Assert.NotEqual(Guid.Empty, actualProject.Id);
        Assert.Equal(expectedDate.Year, actualProject.Year);
        Assert.Equal(expectedDate.Month, actualProject.Month);
        Assert.Equal(4, actualProject.Items.Count);
        Assert.Empty(actualProject.Notes);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var user2 = await UserSeeder.CreateActivatedAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new GetRequest()
        {
            Date = DateTime.Now,
            WorkspaceId = (await _userDao.GetUsersWorkspaces(user2, MembershipAccessType.Owner)).First().Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldGetOnlyWithNotArchived()
    {
        var expectedDate = DateTime.Now.AddMonths(-3);
        
        var existsTracker = await _goalsTrackerSeeder.CreateAsync(_user, _workspace);
        existsTracker.Year = expectedDate.Year;
        existsTracker.Month = expectedDate.Month;
        await _goalsTrackerItemsSeeder.CreateSeveralAsync(existsTracker, 4);
        foreach (var item in await _goalsTrackerItemsSeeder.CreateSeveralAsync(existsTracker, 3))
        {
            await _goalsTrackerItemsDao.Archive(item);
        }
        await CommitDbChanges();
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetRequest()
        {
            Date = expectedDate,
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<GoalsTrackerDto>();
        Assert.NotEqual(Guid.Empty, actualProject.Id);
        Assert.Equal(expectedDate.Year, actualProject.Year);
        Assert.Equal(expectedDate.Month, actualProject.Month);
        Assert.Equal(4, actualProject.Items.Count);
        Assert.Empty(actualProject.Notes);
    }
}
