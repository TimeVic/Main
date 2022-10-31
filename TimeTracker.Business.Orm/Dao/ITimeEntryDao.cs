using Domain.Abstractions;
using NHibernate;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ITimeEntryDao: IDomainService
{
    Task<TimeEntryEntity?> GetByIdAsync(long? id);

    Task DeleteAsync(TimeEntryEntity timeEntry);
    
    Task<TimeEntryEntity> StartNewAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        DateTime date,
        TimeSpan startTime,
        bool isBillable = false,
        string? description = null,
        long? projectId = null,
        decimal? hourlyRate = null
    );
    
    Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        TimeSpan endTime,
        DateTime endDate
    );

    Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );

    Task<bool> HasAccessAsync(UserEntity user, TimeEntryEntity? entity);

    Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace, UserEntity user);

    Task<ICollection<TimeEntryEntity>> GetActiveEntriesAsync(WorkspaceEntity workspace);

    Task<ListDto<TimeEntryEntity>> GetListAsync(
        WorkspaceEntity workspace,
        int page,
        FilterDataDto? filter = null,
        UserEntity? user = null,
        MembershipAccessType accessType = MembershipAccessType.Owner
    );
    
    Task<TimeEntryEntity?> GetActiveEntryForPastDay(
        ISession? session = null,
        CancellationToken cancellationToken = default
    );
}
