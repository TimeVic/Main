using Microsoft.Extensions.Logging;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.Entity;

public class AntiForgetTimerService : IAntiForgetTimerService
{
    private static readonly TimeSpan WarningThreshold = TimeSpan.FromHours(10);
    private static readonly TimeSpan AutoStopThreshold = TimeSpan.FromHours(12);

    private readonly ILogger<AntiForgetTimerService> _logger;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IQueueService _queueService;

    public AntiForgetTimerService(
        ILogger<AntiForgetTimerService> logger,
        ITimeEntryDao timeEntryDao,
        IGcmNotificationService gcmNotificationService,
        IQueueService queueService
    )
    {
        _logger = logger;
        _timeEntryDao = timeEntryDao;
        _gcmNotificationService = gcmNotificationService;
        _queueService = queueService;
    }

    public async Task CheckActiveTimersAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        var timeEntries = await _timeEntryDao.GetActiveEntriesStartedBeforeAsync(currentTime - WarningThreshold);

        foreach (var timeEntry in timeEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (timeEntry.StartTime < currentTime - AutoStopThreshold)
            {
                await _timeEntryDao.AutoStopAsync(timeEntry);
                await _queueService.PushNotificationAsync(
                    new TimeEntryAutoStoppedNotificationItemContext(timeEntry.Id)
                );
                await _queueService.PushExternalClientAsync(
                    new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id)
                );

                _logger.LogInformation("Automatically stopped time entry {TimeEntryId}", timeEntry.Id);
                continue;
            }

            if (timeEntry.AutoStopWarningSentAt.HasValue)
            {
                continue;
            }

            await _gcmNotificationService.SendTimeEntryRunningTooLongNotification(timeEntry);
            timeEntry.AutoStopWarningSentAt = currentTime;
        }
    }
}
