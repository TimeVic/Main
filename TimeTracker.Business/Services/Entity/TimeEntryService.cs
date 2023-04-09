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
    private readonly ILogger<TimeEntryService> _logger;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IQueueService _queueService;
    private readonly IDbSessionProvider _dbSessionProvider;

    public TimeEntryService(
        ILogger<TimeEntryService> logger,
        ITimeEntryDao timeEntryDao,
        IQueueService queueService,
        IDbSessionProvider dbSessionProvider
    )
    {
        _logger = logger;
        _timeEntryDao = timeEntryDao;
        _queueService = queueService;
        _dbSessionProvider = dbSessionProvider;
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
        await _dbSessionProvider.PerformCommitAsync();
        foreach (var timeEntry in timeEntries)
        {
            await _queueService.PushExternalClientAsync(new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id));
        }

        return timeEntries;
    }

    public async Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    )
    {
        var timeEntry = await _timeEntryDao.SetAsync(user, workspace, timeEntryDto, project);
        await _dbSessionProvider.PerformCommitAsync();
        await _queueService.PushExternalClientAsync(new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id));
        return timeEntry;
    }

    public async System.Threading.Tasks.Task DeleteAsync(TimeEntryEntity timeEntry)
    {
        timeEntry.IsMarkedToDelete = true;
        await _dbSessionProvider.CurrentSession.SaveAsync(timeEntry);
        await _dbSessionProvider.PerformCommitAsync();
        await _queueService.PushExternalClientAsync(new SendDeleteTimeEntryIntegrationRequestContext());
    }
}
