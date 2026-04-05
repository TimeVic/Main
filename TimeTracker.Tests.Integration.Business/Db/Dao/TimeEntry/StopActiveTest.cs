using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class StopActiveTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;

    public StopActiveTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace =_userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldStopActive()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime
        );
        Assert.Null(activeEntry.EndTime);
        
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            startTime.AddMinutes(1)
        );
        await FlushDbChanges();
    
        Assert.NotNull(activeEntry.EndTime);
    }

    [Fact]
    public async Task ShouldThrowExceptionIfEndTimeLessThanStartTimeForOneDay()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime
        );
        Assert.Null(activeEntry.EndTime);

        await FlushDbChanges();
        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                startTime.AddSeconds(-1)
            );
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfEndDateLessThanStartDate()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime
        );
        Assert.Null(activeEntry.EndTime);

        await FlushDbChanges();
        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                startTime.AddDays(-1)
            );
        });
    }

    [Fact]
    public async Task ShouldStopActiveOnlyForCurrentUser()
    {
        var startTime = DateTime.UtcNow;

        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
    
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, otherUser, MembershipAccessType.User);
        var otherActiveEntry = await _timeEntryDao.StartNewAsync(otherUser, _workspace, startTime);
    
        await FlushDbChanges();
        var activeEntries = await _timeEntryDao.GetActiveEntriesAsync(_workspace);
        Assert.Equal(2, activeEntries.Count);
        
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            _workspace, 
            _user,
            startTime.AddSeconds(1)
        );
        await FlushDbChanges();
    
        activeEntries = await _timeEntryDao.GetActiveEntriesAsync(_workspace);
        Assert.Equal(1, activeEntries.Count);
        
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.NotNull(activeEntry.EndTime);
        await DbSessionProvider.CurrentSession.RefreshAsync(otherActiveEntry);
        Assert.Null(otherActiveEntry.EndTime);
    }

    [Fact]
    public async Task ShouldThrowExceptionIfEndTimeMoreThanOneDay()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime
        );
        Assert.Null(activeEntry.EndTime);

        await FlushDbChanges();
        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                startTime.AddHours(24)
            );
        });
    }

    [Fact]
    public async Task ShouldNotStopForOtherWorkspace()
    {
        var startTime = DateTime.UtcNow;
        
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        Assert.Null(activeEntry.EndTime);
        
        var workspace2 = await _workspaceDao.CreateWorkspaceAsync(_user, "Test 2");
        await _workspaceAccessService.ShareAccessAsync(workspace2, _user, MembershipAccessType.Owner);
        
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            workspace2,
            _user,
            startTime.AddSeconds(1)
        );
        await FlushDbChanges();
        var stoppedEntry = await _timeEntryDao.GetActiveEntryAsync(workspace2, _user);
        Assert.Null(stoppedEntry);
    }
    
    [Fact]
    public async Task EndTimeShouldBeMoreThanStartTime()
    {
        var startTime = DateTime.UtcNow;
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            startTime.AddSeconds(1)
        );
        await FlushDbChanges();
        
        Assert.True(startedEntry.EndTime >= startedEntry.StartTime);
    }
    
    [Fact]
    public async Task IfEndTimeMoreThanOneDayActiveEntryShouldBeFinishedAndNewEntriesShouldBeCreated()
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddDays(3);
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            endTime
        );

        await FlushDbChanges();
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(4, actualList.TotalCount);

        var lastItem = actualList.Items.First();
        var endTimeOfFirstItem = endTime;
        Assert.Equal(endTimeOfFirstItem.Day, lastItem.EndTime.Value.Day);
        Assert.Equal(endTimeOfFirstItem.Minute, lastItem.EndTime.Value.Minute);
        Assert.Equal(endTimeOfFirstItem.Second, lastItem.EndTime.Value.Second);
        
        var closedEntry = actualList.Items.Last();
        var endOfDayTime = GlobalConstants.EndOfDay;
        Assert.Equal(endOfDayTime.Hours, closedEntry.EndTime.Value.Hour);
        Assert.Equal(endOfDayTime.Minutes, closedEntry.EndTime.Value.Minute);
        Assert.Equal(endOfDayTime.Seconds, closedEntry.EndTime.Value.Second);
    }
    
    [Fact]
    public async Task ShouldNotCreateTooManyItems()
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddDays(100);
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            endTime
        );
        await FlushDbChanges();
        
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(11, actualList.TotalCount);
    }
}
