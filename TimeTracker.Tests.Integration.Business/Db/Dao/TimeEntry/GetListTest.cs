using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
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
}
