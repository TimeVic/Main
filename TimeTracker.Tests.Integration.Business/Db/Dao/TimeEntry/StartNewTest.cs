using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public partial class StartNewTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IProjectDao _projectDao;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITaskSeeder _taskSeeder;
    
    private readonly UserEntity _user;

    public StartNewTest(): base()
    {
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _userDao = Scope.Resolve<IUserDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldStartNewActive()
    {
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay
        );
        Assert.Equal(DateTime.UtcNow.StartOfDay(), activeEntry.Date);
        Assert.Null(activeEntry.EndTime);

        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.Equal(DateTime.UtcNow.StartOfDay(), activeEntry.Date);
        Assert.Null(activeEntry.EndTime);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfActiveExists()
    {
        var workspace1 = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace1,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay
        );
        Assert.Null(activeEntry.EndTime);

        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StartNewAsync(
                _user,
                workspace1,
                DateTime.UtcNow,
                DateTime.UtcNow.TimeOfDay
            );
        });
    }
    
    [Fact]
    public async Task ShouldStartNewForOtherWorkspaceAndDotNotStopForCurrent()
    {
        var workspace1 = await _workspaceDao.CreateWorkspaceAsync(_user, "Test");
        await _workspaceAccessService.ShareAccessAsync(workspace1, _user, MembershipAccessType.Owner);
        var activeEntryFor1 = await _timeEntryDao.StartNewAsync(
            _user,
            workspace1,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay
        );
        
        var workspace2 = await _workspaceDao.CreateWorkspaceAsync(_user, "Test 2");
        await _workspaceAccessService.ShareAccessAsync(workspace2, _user, MembershipAccessType.Owner);
        var activeEntryFor2 = await _timeEntryDao.StartNewAsync(
            _user,
            workspace2,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay
        );
        
        Assert.NotEqual(activeEntryFor1.Id, activeEntryFor2.Id);
        Assert.True(activeEntryFor1.IsActive);
        Assert.True(activeEntryFor2.IsActive);
    }
    
    [Fact]
    public async Task HourlyRateShouldBePastedFromProjectIfNull()
    {
        var expectHourlyRate = 123.56m;
        
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        var project = await _projectDao.CreateAsync(workspace, "test");
        project.DefaultHourlyRate = expectHourlyRate;
        project.IsBillableByDefault = true;
        await DbSessionProvider.PerformCommitAsync();
        
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay,
            isBillable: true,
            projectId: project.Id
        );
        Assert.Equal(project.DefaultHourlyRate, activeEntry.HourlyRate);
        Assert.True(activeEntry.IsBillable);
    }
}
