using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.CompilerServices;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
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
        ProjectEntity rootProjectAlias = null;
        ClientEntity rootClientAlias = null;
        UserEntity rootUserAlias = null;
        var query = Session.QueryOver<TimeEntryEntity>()
            .Inner.JoinAlias(item => item.User, () => rootUserAlias)
            .Left.JoinAlias(item => item.Project, () => rootProjectAlias)
            .Left.JoinAlias(() => rootProjectAlias!.Client, () => rootClientAlias)
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
            ProjectEntity projectAlias = null;
            WorkspaceMembershipProjectAccessEntity projectAccessAlias = null;
            WorkspaceMembershipEntity workspaceMembershipAlias = null;
            UserEntity userAlias = null;
            var allowedIdsSubQuery = QueryOver.Of<TimeEntryEntity>()
                .Inner.JoinAlias(item => item.Project, () => projectAlias)
                .Inner.JoinAlias(item => projectAlias!.MembershipProjectAccess, () => projectAccessAlias)
                .Inner.JoinAlias(() => projectAccessAlias!.WorkspaceMembership, () => workspaceMembershipAlias)
                .And(
                    item => workspaceMembershipAlias!.User.Id == user.Id 
                        && workspaceMembershipAlias.Workspace.Id == workspace.Id
                )
                .Select(
                    Projections.Distinct(
                        Projections.Property<TimeEntryEntity>(s => s.Id)
                    )
                );
            query.WithSubquery
                .WhereProperty(item => item.Id)
                .In(allowedIdsSubQuery);
        }

        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize * 2)
            .ListAsync();
        return new ListDto<TimeEntryEntity>(
            items,
            await query.RowCountAsync()
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

        var items = await Session.Query<TimeEntryEntity>()
            .Where(item => pageItemIds.Contains(item.Id))
            .Fetch(item => item.User)
            .Fetch(item => item.Project)
            .ThenFetch(project => project!.Client)
            .Fetch(item => item.Task)
            .ToListAsync();

        items = items
            .OrderByDescending(item => item.StartTime)
            .ToList();

        return new ListDto<TimeEntryEntity>(items, totalCount);
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
        
        var entry = new TimeEntryEntity
        {
            IsBillable = isBillable,
            Description = description,
            StartTime = startTime,
            EndTime = null,
            Workspace = workspace,
            User = user,
            Task = internalTask,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (internalTask != null)
        {
            projectId = internalTask?.TaskList.Project.Id;
        }
        if (projectId != null)
        {
            entry.Project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
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
        
        foreach (var activeTimeEntry in activeTimeEntries)
        {
            if (activeTimeEntry.StartTime > endTime)
            {
                throw new DataInconsistencyException("End time can not be less than Start time");
            }

            if (activeTimeEntry.StartTime.Date == endTime.Date)
            {
                activeTimeEntry.EndTime = endTime;
                await Session.SaveAsync(activeTimeEntry);
                continue;
            }

            activeTimeEntry.EndTime = activeTimeEntry.StartTime.EndOfDay();
            await Session.FlushAsync();

            var copyStartTime = activeTimeEntry.StartTime.StartOfDay().AddDays(1);
            var createdItemsCount = 0;

            while (copyStartTime.Date <= endTime.Date && createdItemsCount < MaxCreatedItemsIfStopped)
            {
                var newTimeEntry = await StartNewAsync(
                    user,
                    workspace,
                    copyStartTime,
                    isBillable: activeTimeEntry.IsBillable,
                    description: activeTimeEntry.Description,
                    projectId: activeTimeEntry.Project?.Id,
                    hourlyRate: activeTimeEntry.HourlyRate,
                    internalTask: activeTimeEntry.Task
                );

                newTimeEntry.EndTime = copyStartTime.Date == endTime.Date
                    ? endTime
                    : copyStartTime.EndOfDay();

                copyStartTime = copyStartTime.AddDays(1);
                createdItemsCount++;
            }
        }
        
        return activeTimeEntries;
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
            timeEntry = new TimeEntryEntity()
            {
                Workspace = workspace,
                User = user,
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
            if (timeEntryDto.EndTime.HasValue)
            {
                if (timeEntryDto.EndTime.Value - timeEntryDto.StartTime > TimeSpan.FromDays(1))
                {
                    throw new DataInconsistentException("EndTime can not be more than 1 day from StartTime");
                }
                timeEntry.EndTime = timeEntryDto.EndTime;
            }
        }

        await Session.SaveAsync(timeEntry);
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
}
