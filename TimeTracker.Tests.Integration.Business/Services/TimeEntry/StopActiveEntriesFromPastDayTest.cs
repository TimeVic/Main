using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.TimeEntry;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.TimeEntry;

public class StopActiveEntriesFromPastDayTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryService _timeEntryService;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IQueueService _queueService;

    public StopActiveEntriesFromPastDayTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _timeEntryService = Scope.Resolve<ITimeEntryService>();
        _queueService = Scope.Resolve<IQueueService>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task ShouldStopActiveTimeEntryForPastDay()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_workspace, true);
        activeEntry.Date = DateTime.UtcNow.AddDays(-1);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.NotNull(activeEntry.EndTime);
        Assert.Equal(23, activeEntry.EndTime.Value.Hours);
        Assert.Equal(59, activeEntry.EndTime.Value.Minutes);
        Assert.Equal(59, activeEntry.EndTime.Value.Seconds);
        Assert.Equal(999, activeEntry.EndTime.Value.Milliseconds);

        await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(EmailSendingService.IsEmailSent);
        var actualEmail = EmailSendingService.SentMessages.FirstOrDefault();
        Assert.Contains(_user.Email, actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldNotStopActiveForCurrentDay()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_workspace, true);
        var activeEntry2 = await _timeEntryDao.StartNewAsync(_workspace, true);
        activeEntry.Date = DateTime.UtcNow.AddDays(-1);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry2);
        Assert.NotNull(activeEntry.EndTime);
        Assert.Null(activeEntry2.EndTime);
    }
}
