using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface ITimeEntryDao: IDomainService
{
    Task<TimeEntryEntity?> GetByIdAsync(Guid? id);
    
    Task<TimeEntryEntity> StartNewAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        DateTime startTime,
        bool isBillable = false,
        string? description = null,
        Guid? projectId = null,
        decimal? hourlyRate = null,
        TaskEntity? internalTask = null,
        string? timeZoneId = null
    );
    
    Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        DateTime endTime
    );

    Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );
    
    Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace, UserEntity user);

    Task<ICollection<TimeEntryEntity>> GetActiveEntriesAsync(WorkspaceEntity workspace);

    Task<ICollection<TimeEntryEntity>> GetActiveEntriesStartedBeforeAsync(DateTime startedBefore);

    Task AutoStopAsync(TimeEntryEntity timeEntry);

    Task<ListDto<TimeEntryEntity>> GetListAsync(
        WorkspaceEntity workspace,
        int page,
        FilterDataDto? filter = null,
        UserEntity? user = null,
        MembershipAccessType accessType = MembershipAccessType.Owner
    );

    Task<ListDto<TimeEntryEntity>> GetListGroupedByDayAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        int page
    );

    Task<IReadOnlyList<TimeEntryEntity>> GetListInRangeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        DateTime minDate,
        DateTime maxDate,
        CancellationToken cancellationToken = default
    );

    Task<TimeEntryApprovalStatusSummaryDto> GetApprovalStatusSummaryAsync(
        WorkspaceEntity workspace,
        UserEntity user
    );

    Task<IReadOnlyList<TimeEntryApprovalSubmitterItemDto>> GetApprovalSubmittersAsync(
        WorkspaceEntity workspace
    );

    Task<IReadOnlyList<TimeEntryEntity>> GetApprovalDetailsAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        DateTime startDate,
        DateTime endDate
    );
}
