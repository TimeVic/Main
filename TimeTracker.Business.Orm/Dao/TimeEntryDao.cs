using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Orm.Exceptions;

namespace TimeTracker.Business.Orm.Dao;

public class TimeEntryDao: BaseDao, ITimeEntryDao
{
    private const int MaxCreatedItemsIfStopped = 10;
    private const string AutoStoppedDescriptionMarker = "[Auto-stopped]";
    private static readonly TimeSpan AutoStoppedDuration = TimeSpan.FromHours(8);
    
    private readonly ILogger<TimeEntryDao> _logger;
    private readonly IProjectDao _projectDao;

    public TimeEntryDao(
        ILogger<TimeEntryDao> logger,
        IProjectDao projectDao,
        ILifetimeScope scope
    ): base(scope)
    {
        _logger = logger;
        _projectDao = projectDao;
    }

    public async Task<TimeEntryEntity?> GetByIdAsync(Guid? id)
    {
        if (id == null)
            return null;
        return await Session.GetAsync<TimeEntryEntity>(id);
    }  

    public async Task<ListDto<TimeEntryEntity>> GetListAsync(
        WorkspaceEntity workspace,
        int page,
        FilterDataDto? filter = null,
        UserEntity? user = null,
        MembershipAccessType accessType = MembershipAccessType.Owner
    )
    {
        ProjectEntity rootProjectAlias = null!;
        ClientEntity rootClientAlias = null!;
        UserEntity rootUserAlias = null!;
        TaskEntity rootTaskAlias = null!;
        var query = Session.QueryOver<TimeEntryEntity>()
            .Inner.JoinAlias(item => item.User, () => rootUserAlias)
            .Left.JoinAlias(item => item.Project, () => rootProjectAlias)
            .Left.JoinAlias(() => rootProjectAlias!.Client, () => rootClientAlias)
            .Left.JoinAlias(item => item.Task, () => rootTaskAlias)
            .OrderBy(item => item.StartTime).Desc
            .Where(item => item.Workspace.Id == workspace.Id && item.IsMarkedToDelete == false);
        
        if (filter != null)
        {
            if (filter.ClientId.HasValue)
            {
                query = query.And(() => rootClientAlias!.Id == filter.ClientId);
            }
            if (filter.ProjectId.HasValue)
            {
                query = query.And(() => rootProjectAlias!.Id == filter.ProjectId);
            }
            if (filter.TaskId.HasValue)
            {
                query = query.And(() => rootTaskAlias!.Id == filter.TaskId);
            }
            if (filter.IsBillable.HasValue)
            {
                query = query.And(item => item.IsBillable == filter.IsBillable);
            }
            if (filter.MemberId.HasValue)
            {
                query = query.And(() => rootUserAlias!.Id == filter.MemberId);
            }
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.And(
                    Restrictions.Or(
                        Restrictions.InsensitiveLike("Description", filter.Search, MatchMode.Anywhere),
                        Restrictions.InsensitiveLike("TaskId", filter.Search, MatchMode.Anywhere)
                    )
                );
            }
            if (filter.DateFrom.HasValue)
            {
                query = query.And(item => item.StartTime >= filter.DateFrom.Value);
            }
            if (filter.DateTo.HasValue)
            {
                query = query.And(item => item.StartTime <= filter.DateTo.Value);
            }
        }

        if (
            user != null 
            && accessType != MembershipAccessType.Manager 
            && accessType != MembershipAccessType.Owner
        )
        {
            // Is not owner
            ProjectEntity projectAlias = null!;
            WorkspaceMemberProjectAccessEntity projectAccessAlias = null!;
            WorkspaceMemberEntity workspaceMemberAlias = null!;
            var allowedIdsSubQuery = QueryOver.Of<TimeEntryEntity>()
                .Inner.JoinAlias(item => item.Project, () => projectAlias)
                .Inner.JoinAlias(item => projectAlias!.MemberProjectAccess, () => projectAccessAlias)
                .Inner.JoinAlias(() => projectAccessAlias!.WorkspaceMember, () => workspaceMemberAlias)
                .And(
                    item => workspaceMemberAlias!.User.Id == user.Id 
                        && workspaceMemberAlias.Workspace.Id == workspace.Id
                )
                .Select(
                    Projections.Distinct(
                        Projections.Property<TimeEntryEntity>(s => s.Id)
                    )
                );
            query = query.And(
                Restrictions.Or(
                    Subqueries.PropertyIn(
                        nameof(TimeEntryEntity.Id),
                        allowedIdsSubQuery.DetachedCriteria
                    ),
                    Restrictions.Eq(
                        Projections.Property(() => rootUserAlias!.Id),
                        user.Id
                    )
                )
            );
        }

        var offset = PaginationUtils.CalculateOffset(page);
        var totalCount = await query.RowCountAsync();
        var entryIds = await query
            .Select(item => item.Id)
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize * 2)
            .ListAsync<Guid>();
        var items = await LoadListItemsForProfileAsync(entryIds);

        return new ListDto<TimeEntryEntity>(
            items,
            totalCount
        );
    }

    public async Task<ListDto<TimeEntryEntity>> GetListGroupedByDayAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        int page
    )
    {
        var daysInPage = GlobalConstants.TimeEntryGroupedByDayPageSize;
        var dayOffset = PaginationUtils.CalculateOffset(page, daysInPage);
        var totalCount = await Session.Query<TimeEntryEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.User.Id == user.Id)
            .Where(item => item.IsMarkedToDelete == false)
            .Select(item => item.StartTime.Date)
            .Distinct()
            .CountAsync();

        if (totalCount == 0)
        {
            return new ListDto<TimeEntryEntity>(new List<TimeEntryEntity>(), 0);
        }

        var pageItemIds = await Session.CreateSQLQuery(
                ReadSqlQuery("TimeEntry.GetListGroupedByDayIds")
            )
            .AddScalar("id", NHibernateUtil.Guid)
            .SetParameter("workspaceId", workspace.Id)
            .SetParameter("userId", user.Id)
            .SetParameter("dayOffset", dayOffset)
            .SetParameter("dayPageSize", daysInPage)
            .ListAsync<Guid>();

        if (!pageItemIds.Any())
        {
            return new ListDto<TimeEntryEntity>(new List<TimeEntryEntity>(), totalCount);
        }

        var items = await LoadListItemsForProfileAsync(pageItemIds);

        items = items
            .OrderByDescending(item => item.StartTime)
            .ToList();

        return new ListDto<TimeEntryEntity>(items, totalCount);
    }

    private async Task<List<TimeEntryEntity>> LoadListItemsForProfileAsync(ICollection<Guid> entryIds)
    {
        if (!entryIds.Any())
        {
            return new List<TimeEntryEntity>();
        }

        var entries = await Session.Query<TimeEntryEntity>()
            .Where(item => entryIds.Contains(item.Id))
            .Fetch(item => item.User)
            .ThenFetch(user => user.Language)
            .Fetch(item => item.Project)
            .ThenFetch(project => project!.Client)
            .Fetch(item => item.Task)
            .ThenFetch(task => task!.TaskList)
            .ThenFetch(taskList => taskList.Project)
            .ThenFetch(project => project.Client)
            .ThenFetch(client => client.Workspace)
            .Fetch(item => item.Task)
            .ThenFetch(task => task!.User)
            .ThenFetch(user => user.Language)
            .ToListAsync();
        var taskIds = entries
            .Where(item => item.Task != null)
            .Select(item => item.Task!.Id)
            .Distinct()
            .ToList();
        if (taskIds.Any())
        {
            await Session.Query<TaskEntity>()
                .Where(item => taskIds.Contains(item.Id))
                .FetchMany(item => item.Tags)
                .ToListAsync();
        }

        var userIds = entries
            .Select(item => item.User.Id)
            .Concat(entries
                .Where(item => item.Task != null)
                .Select(item => item.Task!.User.Id))
            .Distinct()
            .ToList();
        await Session.Query<UserEntity>()
            .Where(item => userIds.Contains(item.Id))
            .FetchMany(item => item.Avatars)
            .ToListAsync();

        await Session.Query<TimeEntryRejectEntity>()
            .Where(item => entryIds.Contains(item.TimeEntry.Id))
            .Fetch(item => item.User)
            .ToListAsync();

        var entriesById = entries.ToDictionary(item => item.Id);
        return entryIds
            .Select(entryId => entriesById[entryId])
            .ToList();
    }

    
    public async Task<TimeEntryEntity> StartNewAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        DateTime startTime,
        bool isBillable = false,
        string? description = "",
        Guid? projectId = null,
        decimal? hourlyRate = null,
        TaskEntity? internalTask = null
    )
    {
        if (await GetActiveEntryAsync(workspace, user) != null)
        {
            throw new DataInconsistencyException("New time entry can not be created before active exists");
        }
        
        var isApproved = workspace.Mode == WorkspaceMode.Solo || !workspace.IsApprovalsEnabled;
        var entry = new TimeEntryEntity
        {
            IsBillable = isBillable,
            Description = description,
            StartTime = startTime,
            EndTime = null,
            Workspace = workspace,
            User = user,
            Task = internalTask,
            TimeZone = workspace.TimeZone,
            Status = isApproved ? TimeEntryStatus.Approved : TimeEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (internalTask != null)
        {
            entry.Project = internalTask.TaskList.Project;
        }
        else if (projectId != null)
        {
            var project = await _projectDao.GetById(projectId);
            if (project?.Client.Workspace.Id == workspace.Id)
            {
                entry.Project = project;
            }
        }
        entry.HourlyRate = hourlyRate ?? entry.Project?.DefaultHourlyRate;
        await Session.SaveAsync(entry);
        return entry;
    }

    public async Task<ICollection<TimeEntryEntity>> StopActiveAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        DateTime endTime
    )
    {
        var activeTimeEntries = await Session.Query<TimeEntryEntity>()
            .Where(
                item => item.Workspace.Id == workspace.Id 
                    && item.User.Id == user.Id
                    && item.EndTime == null
            )
            .ToListAsync();
        if (activeTimeEntries.Count > 1)
        {
            _logger.LogError(
                "Workspace contains more than one active time entry. WorkspaceId: {WorkspaceId}",
                workspace.Id
            );
        }
        
        var result = new List<TimeEntryEntity>();
        foreach (var activeTimeEntry in activeTimeEntries)
        {
            result.Add(activeTimeEntry);
            if (activeTimeEntry.StartTime > endTime)
            {
                throw new DataInconsistencyException("End time can not be less than Start time");
            }

            // Resolve the timezone for local day-boundary calculations.
            // StartTime/EndTime are stored in UTC; we need to split at midnight in the entry's local timezone.
            TimeZoneInfo timeZoneInfo;
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
                    !string.IsNullOrEmpty(activeTimeEntry.TimeZone) ? activeTimeEntry.TimeZone : "UTC"
                );
            }
            catch (Exception)
            {
                timeZoneInfo = TimeZoneInfo.Utc;
            }

            // Convert UTC timestamps to local time for day-boundary comparison.
            var startTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(activeTimeEntry.StartTime, DateTimeKind.Utc),
                timeZoneInfo
            );
            var endTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(endTime, DateTimeKind.Utc),
                timeZoneInfo
            );

            if (startTimeLocal.Date == endTimeLocal.Date)
            {
                // Same local day — no splitting needed
                activeTimeEntry.EndTime = endTime;
                continue;
            }

            // Multi-day: split at local midnight boundaries.
            // First entry ends at the last moment of its local day (23:59:59.9999999 local → UTC).
            var endOfStartDayLocal = startTimeLocal.Date.AddDays(1).AddTicks(-1);
            activeTimeEntry.EndTime = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(endOfStartDayLocal, DateTimeKind.Unspecified),
                timeZoneInfo
            );
            await Session.FlushAsync();

            // Subsequent entries start at the beginning of each local day (00:00:00 local → UTC).
            var copyStartLocal = startTimeLocal.Date.AddDays(1);
            var createdItemsCount = 0;

            while (copyStartLocal.Date <= endTimeLocal.Date && createdItemsCount < MaxCreatedItemsIfStopped)
            {
                var copyStartUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(copyStartLocal, DateTimeKind.Unspecified),
                    timeZoneInfo
                );

                var newTimeEntry = await StartNewAsync(
                    user,
                    workspace,
                    copyStartUtc,
                    isBillable: activeTimeEntry.IsBillable,
                    description: activeTimeEntry.Description,
                    projectId: activeTimeEntry.Project?.Id,
                    hourlyRate: activeTimeEntry.HourlyRate,
                    internalTask: activeTimeEntry.Task
                );

                if (copyStartLocal.Date == endTimeLocal.Date)
                {
                    newTimeEntry.EndTime = endTime;
                }
                else
                {
                    var endOfThisDayLocal = copyStartLocal.Date.AddDays(1).AddTicks(-1);
                    newTimeEntry.EndTime = TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(endOfThisDayLocal, DateTimeKind.Unspecified),
                        timeZoneInfo
                    );
                }

                result.Add(newTimeEntry);

                copyStartLocal = copyStartLocal.AddDays(1);
                createdItemsCount++;
            }
        }
        
        return result;
    }
    
    public async Task<TimeEntryEntity> SetAsync(
        UserEntity user,
        WorkspaceEntity workspace, 
        TimeEntryCreationDto timeEntryDto, 
        ProjectEntity? project = null
    )
    {
        if (project != null && !workspace.ContainsProject(project))
        {
            throw new DataInconsistentException("Incorrect ProjectId");
        }

        if (timeEntryDto.EndTime < timeEntryDto.StartTime)
        {
            throw new DataInconsistentException("EndTime can not be less than StartTime");
        }
        
        var timeEntry = await Session.Query<TimeEntryEntity>()
            .Where(entry => entry.Id == timeEntryDto.Id)
            .FirstOrDefaultAsync();
        if (timeEntry == null)
        {
            var isApproved = workspace.Mode == WorkspaceMode.Solo || !workspace.IsApprovalsEnabled;
            timeEntry = new TimeEntryEntity()
            {
                Workspace = workspace,
                User = user,
                TimeZone = workspace.TimeZone,
                Status = isApproved ? TimeEntryStatus.Approved : TimeEntryStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        if (timeEntry.Task != null)
        {
            timeEntry.Project = timeEntry.Task.TaskList.Project;
        }
        else
        {
            timeEntry.Project = project;
        }
        
        timeEntry.Description = timeEntryDto.Description;
        timeEntry.HourlyRate = timeEntryDto.HourlyRate;
        timeEntry.IsBillable = timeEntryDto.IsBillable;
        timeEntry.StartTime = timeEntryDto.StartTime;
        
        // According same day
        if (timeEntry.IsNew || !timeEntry.IsActive)
        {
            if (timeEntryDto.EndTime.HasValue && timeEntryDto.EndTime.Value == DateTime.MinValue)
            {
                timeEntryDto.EndTime = null;
            }
            if (timeEntryDto.EndTime.HasValue)
            {
                if (timeEntryDto.EndTime.Value - timeEntryDto.StartTime > TimeSpan.FromDays(1))
                {
                    throw new DataInconsistentException("EndTime can not be more than 1 day from StartTime");
                }
                timeEntry.EndTime = timeEntryDto.EndTime;
            }
        }

        if (timeEntry.IsNew)
        {
            await Session.SaveAsync(timeEntry);
        }
        return timeEntry;
    }

    public async Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace, UserEntity user)
    {
        return await Session.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.Workspace.Id == workspace.Id)
            .Where(entry => entry.User.Id == user.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<TimeEntryEntity>> GetActiveEntriesAsync(WorkspaceEntity workspace)
    {
        return await Session.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.Workspace.Id == workspace.Id)
            .ToListAsync();
    }

    public async Task<ICollection<TimeEntryEntity>> GetActiveEntriesStartedBeforeAsync(DateTime startedBefore)
    {
        var timeEntries = await Session.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.IsMarkedToDelete == false)
            .Where(entry => entry.StartTime < startedBefore)
            .Fetch(entry => entry.Workspace)
            .Fetch(entry => entry.User)
            .ThenFetchMany(user => user.NotificationTokens)
            .ToListAsync();

        return timeEntries
            .DistinctBy(entry => entry.Id)
            .ToList();
    }

    public async Task AutoStopAsync(TimeEntryEntity timeEntry)
    {
        if (!timeEntry.IsActive)
        {
            return;
        }

        timeEntry.EndTime = timeEntry.StartTime.Add(AutoStoppedDuration);
        timeEntry.IsAutostopped = true;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        timeEntry.Description = string.IsNullOrWhiteSpace(timeEntry.Description)
            ? AutoStoppedDescriptionMarker
            : $"{timeEntry.Description.TrimEnd()}\n{AutoStoppedDescriptionMarker}";
    }

    public async Task<IReadOnlyList<TimeEntryEntity>> GetListInRangeAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        DateTime minDate,
        DateTime maxDate,
        CancellationToken cancellationToken = default
    )
    {
        var minDateUtc = DateTime.SpecifyKind(minDate, DateTimeKind.Utc);
        var maxDateUtc = DateTime.SpecifyKind(maxDate, DateTimeKind.Utc);

        return await Session.Query<TimeEntryEntity>()
            .Fetch(e => e.Project)
            .Where(e => e.Workspace.Id == workspace.Id && e.User.Id == user.Id && !e.IsMarkedToDelete)
            .Where(e => e.StartTime >= minDateUtc && e.StartTime <= maxDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<TimeEntryApprovalStatusSummaryDto> GetApprovalStatusSummaryAsync(
        WorkspaceEntity workspace,
        UserEntity user
    )
    {
        var raw = await Session.CreateSQLQuery(ReadSqlQuery("TimeEntry.GetApprovalStatusSummary"))
            .AddScalar("DraftCount", NHibernateUtil.Int32)
            .AddScalar("DraftDurationSeconds", NHibernateUtil.Double)
            .AddScalar("DraftAmount", NHibernateUtil.Decimal)
            .AddScalar("PendingCount", NHibernateUtil.Int32)
            .AddScalar("PendingDurationSeconds", NHibernateUtil.Double)
            .AddScalar("PendingAmount", NHibernateUtil.Decimal)
            .AddScalar("RejectedCount", NHibernateUtil.Int32)
            .AddScalar("LatestRejectionReason", NHibernateUtil.String)
            .SetParameter("workspaceId", workspace.Id)
            .SetParameter("userId", user.Id)
            .SetParameter("statusDraft", (int)TimeEntryStatus.Draft)
            .SetParameter("statusPending", (int)TimeEntryStatus.Pending)
            .SetParameter("statusRejected", (int)TimeEntryStatus.Rejected)
            .SetResultTransformer(Transformers.AliasToBean<TimeEntryApprovalStatusSummaryItemDto>())
            .UniqueResultAsync<TimeEntryApprovalStatusSummaryItemDto>();

        return new TimeEntryApprovalStatusSummaryDto
        {
            DraftCount = raw?.DraftCount ?? 0,
            DraftDuration = TimeSpan.FromSeconds(raw?.DraftDurationSeconds ?? 0),
            DraftAmount = raw?.DraftAmount ?? 0,
            PendingCount = raw?.PendingCount ?? 0,
            PendingDuration = TimeSpan.FromSeconds(raw?.PendingDurationSeconds ?? 0),
            PendingAmount = raw?.PendingAmount ?? 0,
            RejectedCount = raw?.RejectedCount ?? 0,
            LatestRejectionReason = raw?.LatestRejectionReason
        };
    }
}
