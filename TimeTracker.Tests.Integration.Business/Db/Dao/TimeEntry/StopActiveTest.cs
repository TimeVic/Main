using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
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
    public async Task ShouldSplitEntryWhenEndTimeIsMoreThanOneDay()
    {
        // Entries spanning more than 1 day should be split at midnight boundaries, not throw.
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(25); // guaranteed to cross a UTC day boundary

        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(2, list.TotalCount);

        var newestEntry = list.Items.First();
        var oldestEntry = list.Items.Last();

        // Oldest entry ends at the end of its UTC day (23:59:59.xxx UTC).
        Assert.Equal(GlobalConstants.EndOfDay.Hours, oldestEntry.EndTime!.Value.Hour);
        Assert.Equal(GlobalConstants.EndOfDay.Minutes, oldestEntry.EndTime.Value.Minute);
        Assert.Equal(GlobalConstants.EndOfDay.Seconds, oldestEntry.EndTime.Value.Second);

        // Newest entry ends at the requested end time.
        Assert.Equal(endTime.Hour, newestEntry.EndTime!.Value.Hour);
        Assert.Equal(endTime.Minute, newestEntry.EndTime.Value.Minute);
        Assert.Equal(endTime.Second, newestEntry.EndTime.Value.Second);
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
        Assert.Equal(endTimeOfFirstItem.Day, lastItem.EndTime!.Value.Day);
        Assert.Equal(endTimeOfFirstItem.Minute, lastItem.EndTime.Value.Minute);
        Assert.Equal(endTimeOfFirstItem.Second, lastItem.EndTime.Value.Second);
        
        var closedEntry = actualList.Items.Last();
        var endOfDayTime = GlobalConstants.EndOfDay;
        Assert.Equal(endOfDayTime.Hours, closedEntry.EndTime!.Value.Hour);
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

    /// <summary>
    /// Entries that start and end on the same local day (Tokyo UTC+9) but on different UTC days
    /// must NOT be split — the timezone of the time entry governs the day boundary.
    /// </summary>
    [Fact]
    public async Task ShouldNotSplitIfSameDayInLocalTimezoneButDifferentUtcDays()
    {
        const string timeZoneId = "Asia/Tokyo"; // UTC+9, no DST
        _workspace.TimeZone = timeZoneId;
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        // UTC 2024-01-15 23:00 = Tokyo 2024-01-16 08:00 (Jan 16)
        // UTC 2024-01-16 02:00 = Tokyo 2024-01-16 11:00 (Jan 16) — same local day
        var startTime = new DateTime(2024, 1, 15, 23, 0, 0, DateTimeKind.Utc);
        var endTime   = new DateTime(2024, 1, 16,  2, 0, 0, DateTimeKind.Utc);

        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1);
        // Same Tokyo day → single entry, no split.
        Assert.Equal(1, list.TotalCount);

        var entry = list.Items.Single();
        Assert.Equal(endTime.Hour,   entry.EndTime!.Value.Hour);
        Assert.Equal(endTime.Minute, entry.EndTime.Value.Minute);
        Assert.Equal(endTime.Second, entry.EndTime.Value.Second);
    }

    /// <summary>
    /// When the workspace uses a positive UTC offset (UTC+9 Tokyo) the day boundary
    /// must be computed in local time and stored back as UTC.
    /// UTC 2024-01-15 22:00 = Tokyo 2024-01-16 07:00 (Jan 16)
    /// UTC 2024-01-16 20:00 = Tokyo 2024-01-17 05:00 (Jan 17) → 2 entries.
    /// </summary>
    [Fact]
    public async Task ShouldSplitEntriesAtLocalDayBoundaryForPositiveUtcOffset()
    {
        const string timeZoneId = "Asia/Tokyo"; // UTC+9, no DST
        _workspace.TimeZone = timeZoneId;
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        var startTime = new DateTime(2024, 1, 15, 22, 0, 0, DateTimeKind.Utc);
        var endTime   = new DateTime(2024, 1, 16, 20, 0, 0, DateTimeKind.Utc);

        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(2, list.TotalCount);

        // Items ordered descending by StartTime.
        var newestEntry = list.Items.First(); // entry 2: UTC 15:00 → 20:00 Jan 16
        var oldestEntry = list.Items.Last();  // entry 1: UTC 22:00 Jan 15 → 14:59:59 Jan 16

        // Oldest entry must end at Tokyo midnight (23:59:59 local = 14:59:59 UTC).
        Assert.Equal(14, oldestEntry.EndTime!.Value.Hour);
        Assert.Equal(59, oldestEntry.EndTime.Value.Minute);
        Assert.Equal(59, oldestEntry.EndTime.Value.Second);

        // Newest entry must start at 00:00 Tokyo (= 15:00 UTC on Jan 16).
        Assert.Equal(15, newestEntry.StartTime.Hour);
        Assert.Equal( 0, newestEntry.StartTime.Minute);
        Assert.Equal( 0, newestEntry.StartTime.Second);

        // Newest entry ends at the requested end time.
        Assert.Equal(endTime.Hour,   newestEntry.EndTime!.Value.Hour);
        Assert.Equal(endTime.Minute, newestEntry.EndTime.Value.Minute);
    }

    /// <summary>
    /// When the workspace uses a negative UTC offset (UTC-5 New York, January EST)
    /// the day boundary must be computed in local time.
    /// UTC 2024-01-16 02:00 = NY 2024-01-15 21:00 (Jan 15)
    /// UTC 2024-01-16 12:00 = NY 2024-01-16 07:00 (Jan 16) → 2 entries.
    /// </summary>
    [Fact]
    public async Task ShouldSplitEntriesAtLocalDayBoundaryForNegativeUtcOffset()
    {
        const string timeZoneId = "America/New_York"; // UTC-5 in January (EST, no DST)
        _workspace.TimeZone = timeZoneId;
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        var startTime = new DateTime(2024, 1, 16,  2, 0, 0, DateTimeKind.Utc);
        var endTime   = new DateTime(2024, 1, 16, 12, 0, 0, DateTimeKind.Utc);

        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(2, list.TotalCount);

        var newestEntry = list.Items.First(); // entry 2: UTC 05:00 → 12:00 Jan 16
        var oldestEntry = list.Items.Last();  // entry 1: UTC 02:00 → 04:59:59 Jan 16

        // Oldest entry must end at NY midnight (23:59:59 local = 04:59:59 UTC).
        Assert.Equal(4,  oldestEntry.EndTime!.Value.Hour);
        Assert.Equal(59, oldestEntry.EndTime.Value.Minute);
        Assert.Equal(59, oldestEntry.EndTime.Value.Second);

        // Newest entry must start at 00:00 NY (= 05:00 UTC on Jan 16).
        Assert.Equal(5, newestEntry.StartTime.Hour);
        Assert.Equal(0, newestEntry.StartTime.Minute);
        Assert.Equal(0, newestEntry.StartTime.Second);

        // Newest entry ends at the requested end time.
        Assert.Equal(endTime.Hour,   newestEntry.EndTime!.Value.Hour);
        Assert.Equal(endTime.Minute, newestEntry.EndTime.Value.Minute);
    }

    /// <summary>
    /// When the period spans three local days in a non-UTC timezone (Tokyo UTC+9)
    /// three entries must be created with correct day boundaries.
    /// UTC 2024-01-14 22:00 = Tokyo 2024-01-15 07:00 (Jan 15)
    /// UTC 2024-01-16 20:00 = Tokyo 2024-01-17 05:00 (Jan 17) → 3 entries.
    /// </summary>
    [Fact]
    public async Task ShouldSplitMultipleDaysCorrectlyWithNonUtcTimezone()
    {
        const string timeZoneId = "Asia/Tokyo"; // UTC+9, no DST
        _workspace.TimeZone = timeZoneId;
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        var startTime = new DateTime(2024, 1, 14, 22, 0, 0, DateTimeKind.Utc);
        var endTime   = new DateTime(2024, 1, 16, 20, 0, 0, DateTimeKind.Utc);

        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(3, list.TotalCount);

        // Items ordered descending by StartTime.
        var entries = list.Items.OrderBy(e => e.StartTime).ToList();
        var entry1 = entries[0]; // UTC Jan 14 22:00 → UTC Jan 15 14:59:59
        var entry2 = entries[1]; // UTC Jan 15 15:00 → UTC Jan 16 14:59:59
        var entry3 = entries[2]; // UTC Jan 16 15:00 → UTC Jan 16 20:00

        // All entries carry the correct timezone.
        Assert.All(entries, e => Assert.Equal(timeZoneId, e.TimeZone));

        // Entry 1 ends at Tokyo midnight Jan 16 → 14:59:59 UTC Jan 15.
        Assert.Equal(14, entry1.EndTime!.Value.Hour);
        Assert.Equal(59, entry1.EndTime.Value.Minute);
        Assert.Equal(59, entry1.EndTime.Value.Second);

        // Entry 2 starts at Tokyo midnight Jan 16 → 15:00:00 UTC Jan 15.
        Assert.Equal(15, entry2.StartTime.Hour);
        Assert.Equal( 0, entry2.StartTime.Minute);
        // Entry 2 ends at Tokyo midnight Jan 17 → 14:59:59 UTC Jan 16.
        Assert.Equal(14, entry2.EndTime!.Value.Hour);
        Assert.Equal(59, entry2.EndTime.Value.Minute);
        Assert.Equal(59, entry2.EndTime.Value.Second);

        // Entry 3 starts at Tokyo midnight Jan 17 → 15:00:00 UTC Jan 16.
        Assert.Equal(15, entry3.StartTime.Hour);
        Assert.Equal( 0, entry3.StartTime.Minute);
        // Entry 3 ends at the requested end time.
        Assert.Equal(endTime.Hour,   entry3.EndTime!.Value.Hour);
        Assert.Equal(endTime.Minute, entry3.EndTime.Value.Minute);
    }

    [Fact]
    public async Task ShouldKeepEntryTimeZoneWhenWorkspaceTimeZoneChangesBeforeDaySplit()
    {
        const string entryTimeZone = "Asia/Tokyo";
        _workspace.TimeZone = entryTimeZone;
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        var startTime = new DateTime(2026, 1, 15, 22, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2026, 1, 16, 20, 0, 0, DateTimeKind.Utc);
        await _timeEntryDao.StartNewAsync(_user, _workspace, startTime);
        await FlushDbChanges();

        _workspace.TimeZone = "America/New_York";
        await FlushDbChanges();

        await _timeEntryDao.StopActiveAsync(_workspace, _user, endTime);
        await FlushDbChanges();

        var entries = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(2, entries.TotalCount);
        Assert.All(entries.Items, entry => Assert.Equal(entryTimeZone, entry.TimeZone));
    }
}
