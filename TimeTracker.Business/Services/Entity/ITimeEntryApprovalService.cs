using Domain.Abstractions;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Entity;

public interface ITimeEntryApprovalService : IDomainService
{
    Task<TimeEntryEntity> SubmitAsync(UserEntity user, TimeEntryEntity timeEntry);

    Task<IReadOnlyList<TimeEntryEntity>> SubmitPeriodAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        DateTime startDate,
        DateTime endDate
    );

    Task<TimeEntryEntity> ApproveAsync(UserEntity user, TimeEntryEntity timeEntry);

    Task<IReadOnlyList<TimeEntryEntity>> ApproveBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds
    );

    Task<TimeEntryEntity> RejectAsync(UserEntity user, TimeEntryEntity timeEntry, string reason);

    Task<IReadOnlyList<TimeEntryEntity>> RejectBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds,
        string reason
    );

    Task<TimeEntryEntity> UnapproveAsync(UserEntity user, TimeEntryEntity timeEntry);

    Task<IReadOnlyList<TimeEntryEntity>> UnapproveBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds
    );

    Task<TimeEntryApprovalStatusSummaryDto> GetStatusSummaryAsync(UserEntity user, WorkspaceEntity workspace);
}
