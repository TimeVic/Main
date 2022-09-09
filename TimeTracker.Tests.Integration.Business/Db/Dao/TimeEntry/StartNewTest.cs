using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class StartNewTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;

    public StartNewTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
    }

    [Fact]
    public async Task ShouldStartNewActive()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        var activeEntry = await _timeEntryDao.StartNewAsync(workspace, true);
        Assert.Null(activeEntry.EndTime);
    }
    
    [Fact]
    public async Task ShouldStartNewAndStopCurrent()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace1 = user.Workspaces.First();
        var activeEntry = await _timeEntryDao.StartNewAsync(workspace1, true);
        Assert.Null(activeEntry.EndTime);
        
        var actualEntry = await _timeEntryDao.StartNewAsync(workspace1, true);
        Assert.NotEqual(activeEntry.Id, actualEntry.Id);

        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.NotNull(activeEntry.EndTime);
    }
    
    [Fact]
    public async Task ShouldStartNewForOtherWorkspaceAndDotNotStopForCurrent()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        await _workspaceDao.CreateWorkspace(user, "Test");
        var workspace1 = user.Workspaces.First();
        var activeEntryFor1 = await _timeEntryDao.StartNewAsync(workspace1, true);
        var workspace2 = user.Workspaces.Last();
        var activeEntryFor2 = await _timeEntryDao.StartNewAsync(workspace2, true);
        
        Assert.NotEqual(activeEntryFor1.Id, activeEntryFor2.Id);
        Assert.True(activeEntryFor1.IsActive);
        Assert.True(activeEntryFor2.IsActive);
    }
}
