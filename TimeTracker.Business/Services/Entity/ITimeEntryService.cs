using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Entity;

public interface ITimeEntryService: IDomainService
{
    Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        TimeSpan endTime,
        DateTime stopDate
    );

    Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );

    Task DeleteAsync(TimeEntryEntity timeEntry);
}
