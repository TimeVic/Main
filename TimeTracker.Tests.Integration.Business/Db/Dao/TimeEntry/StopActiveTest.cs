using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class StopActiveTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;

    public StopActiveTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
    }

    [Fact]
    public async Task ShouldStopActive()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        var activeEntry = await _timeEntryDao.StartNewAsync(workspace, true);
        Assert.Null(activeEntry.EndTime);
        
        await _timeEntryDao.StopActiveAsync(workspace);

        var actualEntry = await _timeEntryDao.GetByIdAsync(activeEntry.Id);
        Assert.NotNull(actualEntry.EndTime);
    }
    
    [Fact]
    public async Task ShouldNotStopForOtherWorkspace()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        await _workspaceDao.CreateWorkspace(user, "Test");
        var workspace1 = user.Workspaces.First();
        var activeEntry = await _timeEntryDao.StartNewAsync(workspace1, true);
        Assert.Null(activeEntry.EndTime);
        
        var workspace2 = user.Workspaces.Last();
        await _timeEntryDao.StopActiveAsync(workspace2);
        var stoppedEntry = await _timeEntryDao.GetActiveEntryAsync(workspace2);
        Assert.Null(stoppedEntry);
    }
    
    [Fact]
    public async Task EndTimeShouldBeMoreThanStartTime()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace1 = user.Workspaces.First();
        var startedEntry = await _timeEntryDao.StartNewAsync(workspace1, true);
        await _timeEntryDao.StopActiveAsync(workspace1);
        var actualEntry = await _timeEntryDao.GetByIdAsync(startedEntry.Id);
        Assert.True(actualEntry.EndTime >= actualEntry.StartTime);
    }
}
