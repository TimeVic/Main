using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.Entity;

public class TimeEntryService : ITimeEntryService
{
    private readonly TimeSpan _notificationSendingDuration = TimeSpan.FromHours(8);
    
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<TimeEntryService> _logger;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IQueueService _queueService;

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

    public async Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        TimeSpan endTime,
        DateTime stopDate
    )
    {
        var timeEntries = await _timeEntryDao.StopActiveAsync(
            workspace,
            user,
            endTime,
            stopDate
        );
        foreach (var timeEntry in timeEntries)
        {
            await _queueService.PushExternalClientAsync(new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id));
        }

        return timeEntries;
    }

    public async Task<TimeEntryEntity> SetAsync(UserEntity user, WorkspaceEntity workspace, TimeEntryCreationDto timeEntryDto, ProjectEntity? project = null)
    {
        var timeEntry = await _timeEntryDao.SetAsync(user, workspace, timeEntryDto, project);
        await _queueService.PushExternalClientAsync(new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id));
        return timeEntry;
    }
}
