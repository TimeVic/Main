using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ITimeEntryDao: IDomainService
{
    Task<TimeEntryEntity> StartNewAsync(
        WorkspaceEntity workspace,
        bool isBillable,
        string? description = null,
        long? projectId = null
    );
    
    Task<TimeEntryEntity?> StopActiveAsync(WorkspaceEntity workspace);

    Task<TimeEntryEntity> SetAsync(
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );
}
