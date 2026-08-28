using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Entity;

public interface ITimeEntryService: IDomainService
{
    Task<TimeEntryEntity> StartAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        bool isBillable = false,
        string? description = "",
        Guid? projectId = null,
        decimal? hourlyRate = null,
        TaskEntity? internalTask = null
    );

    Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user
    );

    Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );

    Task DeleteAsync(TimeEntryEntity timeEntry);
}
