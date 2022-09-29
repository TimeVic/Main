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
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldStopActiveTimeEntryForPastDay()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        activeEntry.Date = DateTime.UtcNow.AddDays(-1);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.NotNull(activeEntry.EndTime);
        Assert.Equal(23, activeEntry.EndTime.Value.Hours);
        Assert.Equal(59, activeEntry.EndTime.Value.Minutes);
        Assert.Equal(59, activeEntry.EndTime.Value.Seconds);
        Assert.Equal(999, activeEntry.EndTime.Value.Milliseconds);

        var processedCounter = await _queueService.ProcessAsync(QueueChannel.Default);
        Assert.True(processedCounter == 1);
    }
    
    [Fact]
    public async Task ShouldNotStopActiveForCurrentDay()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        
        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        Assert.True(activeEntry.IsActive);
    }
    
    [Fact]
    public async Task ShouldStartNewAfterPreviousWasStopped()
    {
        var previousEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        previousEntry.Date = DateTime.UtcNow.AddDays(-1);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(previousEntry);
        Assert.False(previousEntry.IsActive);

        var newEntry = await _timeEntryDao.GetActiveEntryAsync(_workspace);
        Assert.NotNull(newEntry);
        Assert.True(newEntry.IsActive);
        
        Assert.Equal(previousEntry.Description, newEntry.Description);
        Assert.Equal(previousEntry.IsBillable, newEntry.IsBillable);
        Assert.Equal(previousEntry.HourlyRate, newEntry.HourlyRate);
        Assert.Equal(previousEntry.Project?.Id, newEntry.Project?.Id);
    }
    
    [Fact]
    public async Task ShouldNotSendNotificationIfDurationLessThan8Hours()
    {
        var previousEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        previousEntry.Date = DateTime.UtcNow.AddDays(-1);
        previousEntry.StartTime = TimeSpan.FromHours(23 - 7);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.False(EmailSendingServiceMock.IsEmailSent);
    }
    
    [Fact]
    public async Task ShouldNotSendNotificationIfDurationMoreThan8Hours()
    {
        var previousEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        previousEntry.Date = DateTime.UtcNow.AddDays(-1);
        previousEntry.StartTime = TimeSpan.FromHours(10);
        await DbSessionProvider.PerformCommitAsync();

        await _timeEntryService.StopActiveEntriesFromPastDayAsync();
        
        await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(_user.Email, actualEmail.To);
    }
}
