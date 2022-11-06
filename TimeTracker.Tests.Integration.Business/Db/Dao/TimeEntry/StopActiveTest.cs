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
        var startTime = DateTimeOffset.UtcNow.TimeOfDay;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow, 
            DateTimeOffset.UtcNow.TimeOfDay
        );
        Assert.Null(activeEntry.EndTime);
        
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            startTime + TimeSpan.FromMinutes(1),
            DateTimeOffset.UtcNow.Date
        );
        await CommitDbChanges();
    
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.NotNull(activeEntry.EndTime);
    }

    [Fact]
    public async Task ShouldThrowExceptionIfEndTimeLessThanStartTimeForOneDay()
    {
        var startTime = DateTimeOffset.UtcNow.TimeOfDay;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow, 
            DateTimeOffset.UtcNow.TimeOfDay
        );
        Assert.Null(activeEntry.EndTime);

        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                startTime + TimeSpan.FromSeconds(-1),
                DateTimeOffset.UtcNow.Date
            );
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfEndDateLessThanStartDate()
    {
        var startTime = DateTimeOffset.UtcNow.TimeOfDay;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow, 
            DateTimeOffset.UtcNow.TimeOfDay
        );
        Assert.Null(activeEntry.EndTime);

        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                startTime + TimeSpan.FromSeconds(5),
                DateTimeOffset.UtcNow.Date.AddDays(-1)
            );
        });
    }

    [Fact]
    public async Task ShouldStopActiveOnlyForCurrentUser()
    {
        var date = DateTime.UtcNow;
        var startTime = DateTime.UtcNow.TimeOfDay;

        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, date, startTime);
    
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, otherUser, MembershipAccessType.User);
        var otherActiveEntry = await _timeEntryDao.StartNewAsync(otherUser, _workspace, date, startTime);
    
        var activeEntries = await _timeEntryDao.GetActiveEntriesAsync(_workspace);
        Assert.Equal(2, activeEntries.Count);
        
        await _timeEntryDao.StopActiveAsync(
            _workspace, 
            _user,
            startTime + TimeSpan.FromSeconds(1),
            date
        );
        await CommitDbChanges();
    
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
        var startTime = DateTimeOffset.UtcNow.TimeOfDay;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow, 
            DateTimeOffset.UtcNow.TimeOfDay
        );
        Assert.Null(activeEntry.EndTime);

        await Assert.ThrowsAsync<DataInconsistencyException>(async () =>
        {
            await _timeEntryDao.StopActiveAsync(
                _workspace,
                _user,
                TimeSpan.FromHours(24),
                DateTimeOffset.UtcNow.Date
            );
        });
    }

    [Fact]
    public async Task ShouldNotStopForOtherWorkspace()
    {
        var date = DateTime.UtcNow;
        var startTime = DateTime.UtcNow.TimeOfDay;
        
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, date, startTime);
        Assert.Null(activeEntry.EndTime);
        
        var workspace2 = await _workspaceDao.CreateWorkspaceAsync(_user, "Test 2");
        await _workspaceAccessService.ShareAccessAsync(workspace2, _user, MembershipAccessType.Owner);
        await _timeEntryDao.StopActiveAsync(
            workspace2,
            _user,
            startTime + TimeSpan.FromSeconds(1),
            date
        );
        var stoppedEntry = await _timeEntryDao.GetActiveEntryAsync(workspace2, _user);
        Assert.Null(stoppedEntry);
    }
    
    [Fact]
    public async Task EndTimeShouldBeMoreThanStartTime()
    {
        var date = DateTime.UtcNow;
        var startTime = DateTime.UtcNow.TimeOfDay;
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, date, startTime);
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            startTime + TimeSpan.FromSeconds(1),
            date
        );
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(startedEntry);
        Assert.True(startedEntry.EndTime >= startedEntry.StartTime);
    }
    
    [Fact]
    public async Task IfEndTimeMoreThanOneDayActiveEntryShouldBeFinishedAndNewEntriesShouldBeCreated()
    {
        var date = DateTime.UtcNow.StartOfDay();
        var startTime = DateTime.UtcNow.TimeOfDay;
        var endTime = startTime;
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, date, startTime);
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            endTime,
            date.AddDays(3)
        );
        await CommitDbChanges();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(startedEntry);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(4, actualList.TotalCount);

        var lastItem = actualList.Items.First();
        Assert.Equal(date.AddDays(3), lastItem.Date);
        var endTimeOfFirstItem = endTime;
        Assert.Equal(endTimeOfFirstItem.Days, lastItem.EndTime.Value.Days);
        Assert.Equal(endTimeOfFirstItem.Minutes, lastItem.EndTime.Value.Minutes);
        Assert.Equal(endTimeOfFirstItem.Seconds, lastItem.EndTime.Value.Seconds);
        
        var closedEntry = actualList.Items.Last();
        Assert.Equal(date, closedEntry.Date);
        Assert.Equal(GlobalConstants.EndOfDay.Hours, closedEntry.EndTime.Value.Hours);
        Assert.Equal(GlobalConstants.EndOfDay.Minutes, closedEntry.EndTime.Value.Minutes);
        Assert.Equal(GlobalConstants.EndOfDay.Seconds, closedEntry.EndTime.Value.Seconds);
    }
    
    [Fact]
    public async Task ShouldNotCreateTooManyItems()
    {
        var date = DateTime.UtcNow.StartOfDay();
        var startTime = DateTime.UtcNow.TimeOfDay;
        var endTime = startTime;
        
        var startedEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, date, startTime);
        await _timeEntryDao.StopActiveAsync(
            _workspace,
            _user,
            endTime,
            date.AddDays(100)
        );
        await CommitDbChanges();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(startedEntry);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(11, actualList.TotalCount);
    }
}
