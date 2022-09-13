using Microsoft.Extensions.Logging;
using NHibernate;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.TimeEntry;

public class TimeEntryService : ITimeEntryService
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<TimeEntryService> _logger;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IQueueService _queueService;

    private static readonly TimeSpan EndOfDay = TimeSpan.FromHours(23)
        .Add(TimeSpan.FromMinutes(59))
        .Add(TimeSpan.FromSeconds(59))
        .Add(TimeSpan.FromMilliseconds(999));

    public TimeEntryService(
        IDbSessionProvider sessionProvider,
        ILogger<TimeEntryService> logger,
        ITimeEntryDao timeEntryDao,
        IQueueService queueService
    )
    {
        _sessionProvider = sessionProvider;
        _logger = logger;
        _timeEntryDao = timeEntryDao;
        _queueService = queueService;
    }

    public async Task StopActiveEntriesFromPastDayAsync()
    {
        try
        {
            TimeEntryEntity? activeEntity = null;
            while (true)
            {
                activeEntity = await _timeEntryDao.GetActiveEntryForPastDay();
                if (activeEntity == null)
                {
                    break;
                }

                activeEntity.EndTime = EndOfDay;
                await _queueService.PushNotificationAsync(
                    new TimeEntryAutoStoppedNotificationContext(activeEntity.Workspace.User.Email)
                );
                await _sessionProvider.PerformCommitAsync();

                await _timeEntryDao.StartNewAsync(
                    activeEntity.Workspace,
                    activeEntity.IsBillable,
                    activeEntity.Description,
                    activeEntity.Project?.Id
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
