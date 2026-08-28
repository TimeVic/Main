using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;

namespace TimeTracker.Business.Services.Entity;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IQueueService _queueService;

    public TimeEntryService(
        ITimeEntryDao timeEntryDao,
        IQueueService queueService
    )
    {
        _timeEntryDao = timeEntryDao;
        _queueService = queueService;
    }

    public async Task<TimeEntryEntity> StartAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        bool isBillable = false,
        string? description = "",
        Guid? projectId = null,
        decimal? hourlyRate = null,
        TaskEntity? internalTask = null
    )
    {
        return await _timeEntryDao.StartNewAsync(
            user,
            workspace,
            DateTime.UtcNow,
            isBillable,
            description,
            projectId,
            hourlyRate,
            internalTask
        );
    }

    public async Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user
    )
    {
        var timeEntries = await _timeEntryDao.StopActiveAsync(
            workspace,
            user,
            DateTime.UtcNow
        );
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
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(workspace.TimeZone);
        timeEntryDto.StartTime = ConvertWallClockToUtc(timeEntryDto.StartTime, timeZone);
        timeEntryDto.EndTime = ConvertWallClockToUtc(timeEntryDto.EndTime, timeZone);

        var timeEntry = await _timeEntryDao.SetAsync(user, workspace, timeEntryDto, project);
        await _queueService.PushExternalClientAsync(new SendSetTimeEntryIntegrationRequestContext(timeEntry.Id));
        return timeEntry;
    }

    private static DateTime ConvertWallClockToUtc(DateTime value, TimeZoneInfo timeZone)
    {
        var wallClock = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

        // During a spring DST transition some wall-clock values do not exist.
        // Move them to the first valid minute rather than rejecting the save.
        while (timeZone.IsInvalidTime(wallClock))
        {
            wallClock = wallClock.AddMinutes(1);
        }

        if (timeZone.IsAmbiguousTime(wallClock))
        {
            // Choose the later occurrence deterministically (the standard-time offset).
            var offset = timeZone.GetAmbiguousTimeOffsets(wallClock).Min();
            return new DateTimeOffset(wallClock, offset).UtcDateTime;
        }

        return TimeZoneInfo.ConvertTimeToUtc(wallClock, timeZone);
    }

    private static DateTime? ConvertWallClockToUtc(DateTime? value, TimeZoneInfo timeZone)
    {
        return value.HasValue ? ConvertWallClockToUtc(value.Value, timeZone) : null;
    }

    public async Task DeleteAsync(TimeEntryEntity timeEntry)
    {
        timeEntry.IsMarkedToDelete = true;
        await _queueService.PushExternalClientAsync(new SendDeleteTimeEntryIntegrationRequestContext());
    }

}
