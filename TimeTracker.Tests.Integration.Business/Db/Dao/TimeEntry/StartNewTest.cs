using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public partial class StartNewTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IProjectSeeder _projectSeeder;
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
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
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
            DateTime.UtcNow
        );
        Assert.Null(activeEntry.EndTime);

        await FlushDbChanges();
        Assert.Null(activeEntry.EndTime);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfActiveExists()
    {
        var workspace1 = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace1,
            DateTime.UtcNow
        );
        Assert.Null(activeEntry.EndTime);

        await FlushDbChanges();
        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StartNewAsync(
                _user,
                workspace1,
                DateTime.UtcNow
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
            DateTime.UtcNow
        );
        
        var workspace2 = await _workspaceDao.CreateWorkspaceAsync(_user, "Test 2");
        await _workspaceAccessService.ShareAccessAsync(workspace2, _user, MembershipAccessType.Owner);
        var activeEntryFor2 = await _timeEntryDao.StartNewAsync(
            _user,
            workspace2,
            DateTime.UtcNow
        );
        
        await FlushDbChanges();
        Assert.NotEqual(activeEntryFor1.Id, activeEntryFor2.Id);
        Assert.True(activeEntryFor1.IsActive);
        Assert.True(activeEntryFor2.IsActive);
    }
    
    [Fact]
    public async Task HourlyRateShouldBePastedFromProjectIfNull()
    {
        var expectHourlyRate = 123.56m;
        
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        var project = await _projectSeeder.CreateAsync(workspace);
        project.DefaultHourlyRate = expectHourlyRate;
        project.IsBillableByDefault = true;
        await FlushDbChanges();
        
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            isBillable: true,
            projectId: project.Id
        );
        await FlushDbChanges();
        
        Assert.Equal(project.DefaultHourlyRate, activeEntry.HourlyRate);
        Assert.True(activeEntry.IsBillable);
    }
}
