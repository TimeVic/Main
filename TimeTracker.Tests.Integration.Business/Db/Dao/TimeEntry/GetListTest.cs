using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IProjectSeeder _projectSeeder;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _timeEntrySeeder.CreateSeveralAsync(user, expectedCounter);

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
        
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.True(item.CreateTime > DateTime.MinValue);
            Assert.True(item.EndTime > TimeSpan.MinValue);
            Assert.NotNull(item.Project);
        });
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherNamespaces()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _timeEntrySeeder.CreateSeveralAsync(user, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        await _timeEntrySeeder.CreateSeveralAsync(user2, 15);
        
        var actualList = await _timeEntryDao.GetListAsync(workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldSortList()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _timeEntrySeeder.CreateSeveralAsync(user, 3);

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1);

        var actualFirst = actualList.Items.First();
        var actualLast = actualList.Items.Last();
        Assert.True(actualFirst.StartTime > actualLast.StartTime);
    }
    
    [Fact]
    public async Task ShouldFilterByClient()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _timeEntrySeeder.CreateSeveralAsync(user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(user)).First();
        await _timeEntrySeeder.CreateSeveralAsync(user, expectedCounter, expectedProject);

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1, new FilterDataDto()
        {
            ClientId = expectedProject.Client.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByProject()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _timeEntrySeeder.CreateSeveralAsync(user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(user)).First();
        await _timeEntrySeeder.CreateSeveralAsync(user, expectedCounter, expectedProject);

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1, new FilterDataDto()
        {
            ProjectId = expectedProject.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBillable()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(user, 9))
        {
            timeEntryEntity.IsBillable = false;
        }
        
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(user, expectedCounter))
        {
            timeEntryEntity.IsBillable = true;
        }
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1, new FilterDataDto()
        {
            IsBillable = true
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBySearchString()
    {
        var expectedDescription = "Some fake desc";
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(user, 9)).First();
        expectedEntry.Description = expectedDescription;
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(workspace, 1, new FilterDataDto()
        {
            Search = "FAKE"
        });
        Assert.Equal(1, actualList.TotalCount);
        Assert.Equal(expectedEntry.Id, actualList.Items.First().Id);
    }
}
