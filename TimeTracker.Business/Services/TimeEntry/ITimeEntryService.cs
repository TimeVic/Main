using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.TimeEntry;

public interface ITimeEntryService: IDomainService
{
    Task<ICollection<TimeEntryEntity>> StopActiveAsync(WorkspaceEntity workspace);

    Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );
    
    Task StopActiveEntriesFromPastDayAsync(CancellationToken cancellationToken = default);
}
