using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Notifications.Senders.TimeEntry;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Entity;

public class TimeEntryApprovalService : ITimeEntryApprovalService
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IQueueService _queueService;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ILogger<TimeEntryApprovalService> _logger;

    public TimeEntryApprovalService(
        IDbSessionProvider sessionProvider,
        IWorkspaceAccessService workspaceAccessService,
        IQueueService queueService,
        ITimeEntryDao timeEntryDao,
        ILogger<TimeEntryApprovalService> logger
    )
    {
        _sessionProvider = sessionProvider;
        _workspaceAccessService = workspaceAccessService;
        _queueService = queueService;
        _timeEntryDao = timeEntryDao;
        _logger = logger;
    }

    public async Task<TimeEntryEntity> SubmitAsync(UserEntity user, TimeEntryEntity timeEntry)
    {
        if (timeEntry.User.Id != user.Id)
        {
            throw new HasNoAccessException();
        }

        if (timeEntry.EndTime == null)
        {
            throw new RecordCanNotBeModifiedException("Active time entry cannot be submitted for approval");
        }

        if (timeEntry.Status is not (TimeEntryStatus.Draft or TimeEntryStatus.Rejected))
        {
            throw new RecordCanNotBeModifiedException("Only Draft or Rejected time entries can be submitted");
        }

        timeEntry.Status = TimeEntryStatus.Pending;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        return timeEntry;
    }

    public async Task<IReadOnlyList<TimeEntryEntity>> SubmitPeriodAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        DateTime startDate,
        DateTime endDate
    )
    {
        var entries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(e => e.Workspace.Id == workspace.Id && e.User.Id == user.Id && !e.IsMarkedToDelete)
            .Where(e => e.Status == TimeEntryStatus.Draft || e.Status == TimeEntryStatus.Rejected)
            .Where(e => e.EndTime != null)
            .Where(e => e.StartTime >= startDate && e.StartTime <= endDate)
            .ToListAsync();

        foreach (var entry in entries)
        {
            entry.Status = TimeEntryStatus.Pending;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        return entries;
    }

    public async Task<TimeEntryEntity> ApproveAsync(UserEntity user, TimeEntryEntity timeEntry)
    {
        await AssertHasManagerAccessAsync(user, timeEntry.Workspace);

        if (timeEntry.EndTime == null)
        {
            throw new RecordCanNotBeModifiedException("Active time entry cannot be approved");
        }

        timeEntry.Status = TimeEntryStatus.Approved;
        timeEntry.UpdatedAt = DateTime.UtcNow;

        var approval = new TimeEntryApprovalEntity
        {
            TimeEntry = timeEntry,
            User = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        timeEntry.Approvals.Add(approval);

        await _sessionProvider.CurrentSession.SaveAsync(approval);
        return timeEntry;
    }

    public async Task<IReadOnlyList<TimeEntryEntity>> ApproveBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds
    )
    {
        await AssertHasManagerAccessAsync(user, workspace);

        if (!timeEntryIds.Any())
        {
            return new List<TimeEntryEntity>();
        }

        var entries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(e => e.Workspace.Id == workspace.Id && timeEntryIds.Contains(e.Id) && !e.IsMarkedToDelete)
            .Where(e => e.EndTime != null)
            .ToListAsync();

        foreach (var entry in entries)
        {
            entry.Status = TimeEntryStatus.Approved;
            entry.UpdatedAt = DateTime.UtcNow;

            var approval = new TimeEntryApprovalEntity
            {
                TimeEntry = entry,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            entry.Approvals.Add(approval);

            await _sessionProvider.CurrentSession.SaveAsync(approval);
        }

        return entries;
    }

    public async Task<TimeEntryEntity> RejectAsync(UserEntity user, TimeEntryEntity timeEntry, string reason)
    {
        await AssertHasManagerAccessAsync(user, timeEntry.Workspace);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new RecordCanNotBeModifiedException("Rejection reason is required");
        }

        timeEntry.Status = TimeEntryStatus.Rejected;
        timeEntry.UpdatedAt = DateTime.UtcNow;

        var rejection = new TimeEntryRejectEntity
        {
            TimeEntry = timeEntry,
            User = user,
            Reason = reason.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        timeEntry.Rejections.Add(rejection);

        await _sessionProvider.CurrentSession.SaveAsync(rejection);

        await _queueService.PushNotificationAsync(new TimeEntriesRejectedNotificationItemContext(
            timeEntry.User.Id,
            timeEntry.Workspace.Id,
            reason.Trim(),
            new List<Guid> { timeEntry.Id }
        ));

        return timeEntry;
    }

    public async Task<IReadOnlyList<TimeEntryEntity>> RejectBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds,
        string reason
    )
    {
        await AssertHasManagerAccessAsync(user, workspace);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new RecordCanNotBeModifiedException("Rejection reason is required");
        }

        if (!timeEntryIds.Any())
        {
            return new List<TimeEntryEntity>();
        }

        var entries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(e => e.Workspace.Id == workspace.Id && timeEntryIds.Contains(e.Id) && !e.IsMarkedToDelete)
            .Fetch(e => e.User)
            .ToListAsync();

        var rejectedEntries = new List<TimeEntryEntity>();
        var userEntryMap = new Dictionary<Guid, List<Guid>>();

        foreach (var entry in entries)
        {
            entry.Status = TimeEntryStatus.Rejected;
            entry.UpdatedAt = DateTime.UtcNow;

            var rejection = new TimeEntryRejectEntity
            {
                TimeEntry = entry,
                User = user,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            entry.Rejections.Add(rejection);

            await _sessionProvider.CurrentSession.SaveAsync(rejection);
            rejectedEntries.Add(entry);

            if (!userEntryMap.ContainsKey(entry.User.Id))
            {
                userEntryMap[entry.User.Id] = new List<Guid>();
            }
            userEntryMap[entry.User.Id].Add(entry.Id);
        }

        foreach (var (authorId, ids) in userEntryMap)
        {
            await _queueService.PushNotificationAsync(new TimeEntriesRejectedNotificationItemContext(
                authorId,
                workspace.Id,
                reason.Trim(),
                ids
            ));
        }

        return rejectedEntries;
    }

    public async Task<TimeEntryEntity> UnapproveAsync(UserEntity user, TimeEntryEntity timeEntry)
    {
        await AssertHasManagerAccessAsync(user, timeEntry.Workspace);

        timeEntry.Status = TimeEntryStatus.Draft;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        return timeEntry;
    }

    public async Task<IReadOnlyList<TimeEntryEntity>> UnapproveBatchAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        ICollection<Guid> timeEntryIds
    )
    {
        await AssertHasManagerAccessAsync(user, workspace);

        if (!timeEntryIds.Any())
        {
            return new List<TimeEntryEntity>();
        }

        var entries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(e => e.Workspace.Id == workspace.Id && timeEntryIds.Contains(e.Id) && !e.IsMarkedToDelete)
            .ToListAsync();

        foreach (var entry in entries)
        {
            entry.Status = TimeEntryStatus.Draft;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        return entries;
    }

    public async Task<TimeEntryApprovalStatusSummaryDto> GetStatusSummaryAsync(UserEntity user, WorkspaceEntity workspace)
    {
        return await _timeEntryDao.GetApprovalStatusSummaryAsync(workspace, user);
    }

    private async Task AssertHasManagerAccessAsync(UserEntity user, WorkspaceEntity workspace)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        if (accessType is not (MembershipAccessType.Owner or MembershipAccessType.Manager))
        {
            throw new HasNoAccessException();
        }
    }
}
