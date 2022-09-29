using Microsoft.Extensions.Logging;
using NHibernate;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.TimeEntry;

public class TimeEntryService : ITimeEntryService
{
    private readonly TimeSpan _notificationSendingDuration = TimeSpan.FromHours(8);
    
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

    public async Task<ICollection<TimeEntryEntity>> StopActiveAsync(WorkspaceEntity workspace)
    {
        var timeEntries = await _timeEntryDao.StopActiveAsync(workspace);
        foreach (var timeEntry in timeEntries)
        {
            await _queueService.PushDefaultAsync(new IntegrationAppQueueItemContext(timeEntry.Id));
        }

        return timeEntries;
    }

    public async Task<TimeEntryEntity> SetAsync(UserEntity user, WorkspaceEntity workspace, TimeEntryCreationDto timeEntryDto, ProjectEntity? project = null)
    {
        var timeEntry = await _timeEntryDao.SetAsync(user, workspace, timeEntryDto, project);
        await _queueService.PushDefaultAsync(new IntegrationAppQueueItemContext(timeEntry.Id));
        return timeEntry;
    }

    public async Task StopActiveEntriesFromPastDayAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            TimeEntryEntity? activeEntity = null;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                activeEntity = await _timeEntryDao.GetActiveEntryForPastDay(null, cancellationToken);
                if (activeEntity == null)
                {
                    break;
                }

                activeEntity.EndTime = EndOfDay;
                if (activeEntity.Duration >= _notificationSendingDuration)
                {
                    await _queueService.PushNotificationAsync(
                        new TimeEntryAutoStoppedNotificationItemContext(activeEntity.Workspace.User.Email)
                    );    
                }
                await _sessionProvider.PerformCommitAsync(cancellationToken);

                await _timeEntryDao.StartNewAsync(
                    activeEntity.User,
                    activeEntity.Workspace,
                    activeEntity.IsBillable,
                    activeEntity.Description,
                    activeEntity.Project?.Id
                );
                await _sessionProvider.PerformCommitAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
    }
}
