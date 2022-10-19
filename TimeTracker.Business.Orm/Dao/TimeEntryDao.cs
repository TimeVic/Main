﻿using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Exceptions;

namespace TimeTracker.Business.Orm.Dao;

public class TimeEntryDao: ITimeEntryDao
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<TimeEntryDao> _logger;
    private readonly IProjectDao _projectDao;

    public TimeEntryDao(
        IDbSessionProvider sessionProvider,
        ILogger<TimeEntryDao> logger,
        IProjectDao projectDao
    )
    {
        _sessionProvider = sessionProvider;
        _logger = logger;
        _projectDao = projectDao;
    }

    public async Task<TimeEntryEntity?> GetByIdAsync(long? id)
    {
        if (id == null)
            return null;
        return await _sessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(id);
    }
    
    public async Task DeleteAsync(TimeEntryEntity timeEntry)
    {
        await _sessionProvider.CurrentSession.DeleteAsync(timeEntry);
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
        var query = _sessionProvider.CurrentSession.QueryOver<TimeEntryEntity>()
            .Inner.JoinAlias(item => item.User, () => rootUserAlias)
            .Left.JoinAlias(item => item.Project, () => rootProjectAlias)
            .Left.JoinAlias(() => rootProjectAlias.Client, () => rootClientAlias)
            .OrderBy(item => item.Date).Desc
            .OrderBy(item => item.StartTime).Desc
            .Where(item => item.Workspace.Id == workspace.Id);
        
        if (filter != null)
        {
            if (filter.ClientId.HasValue)
            {
                query = query.And(() => rootClientAlias.Id == filter.ClientId);
            }
            if (filter.ProjectId.HasValue)
            {
                query = query.And(() => rootProjectAlias.Id == filter.ProjectId);
            }
            if (filter.IsBillable.HasValue)
            {
                query = query.And(item => item.IsBillable == filter.IsBillable);
            }
            if (filter.MemberId.HasValue)
            {
                query = query.And(() => rootUserAlias.Id == filter.MemberId);
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
                var startOfDay = filter.DateFrom.Value.StartOfDay();
                query = query.And(item => item.Date >= startOfDay);
            }
            if (filter.DateTo.HasValue)
            {
                var endOfDay = filter.DateTo.Value.EndOfDay();
                query = query.And(item => item.Date <= endOfDay);
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
                .Inner.JoinAlias(item => projectAlias.MembershipProjectAccess, () => projectAccessAlias)
                .Inner.JoinAlias(() => projectAccessAlias.WorkspaceMembership, () => workspaceMembershipAlias)
                .And(
                    item => workspaceMembershipAlias.User.Id == user.Id 
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
            .Take(GlobalConstants.ListPageSize)
            .ListAsync();
        return new ListDto<TimeEntryEntity>(
            items,
            await query.RowCountAsync()
        );
    }
    
    public async Task<TimeEntryEntity> StartNewAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        bool isBillable = false,
        string? description = "",
        long? projectId = null,
        decimal? hourlyRate = null
    )
    {
        await StopActiveAsync(workspace, user);
        
        var entry = new TimeEntryEntity
        {
            IsBillable = isBillable,
            Description = description,
            Date = DateTime.UtcNow.StartOfDay(),
            StartTime = DateTime.UtcNow.TimeOfDay,
            EndTime = null,
            Workspace = workspace,
            User = user,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        if (projectId != null)
        {
            entry.Project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        }
        entry.HourlyRate = hourlyRate ?? entry.Project?.DefaultHourlyRate;
        await _sessionProvider.CurrentSession.SaveAsync(entry);

        return entry;
    }

    public async Task<ICollection<TimeEntryEntity>> StopActiveAsync(WorkspaceEntity workspace, UserEntity user)
    {
        var activeTimeEntries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
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

        foreach (var timeEntry in activeTimeEntries)
        {
            timeEntry.EndTime = DateTime.UtcNow.TimeOfDay;
            await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
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

        var timeEntry = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(entry => entry.Id == timeEntryDto.Id)
            .FirstOrDefaultAsync();
        if (timeEntry == null)
        {
            timeEntry = new TimeEntryEntity()
            {
                Workspace = workspace,
                User = user,
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };
        }
        timeEntry.Project = project;
        timeEntry.TaskId = timeEntryDto.TaskId;
        timeEntry.Description = timeEntryDto.Description;
        timeEntry.HourlyRate = timeEntryDto.HourlyRate;
        timeEntry.IsBillable = timeEntryDto.IsBillable;
        timeEntry.StartTime = timeEntryDto.StartTime;
        if (timeEntryDto.Date > DateTime.MinValue)
        {
            timeEntry.Date = timeEntryDto.Date;    
        }
        if (timeEntry.IsNew || !timeEntry.IsActive)
        {
            timeEntry.EndTime = timeEntryDto.EndTime;    
        }

        await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
        return timeEntry;
    }

    public async Task<bool> HasAccessAsync(UserEntity user, TimeEntryEntity? entity)
    {
        if (entity == null)
        {
            return false;
        }

        TimeEntryEntity timeEntryAlias = null;
        var itemsWithAccessCount = await _sessionProvider.CurrentSession.QueryOver<WorkspaceEntity>()
            .Inner.JoinAlias(item => item.TimeEntries, () => timeEntryAlias)
                .Where(item => item.Owner.Id == user.Id)
                .And(() => timeEntryAlias.Id == entity.Id)
            .RowCountAsync();
        return itemsWithAccessCount > 0;
    }
    
    public async Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace, UserEntity user)
    {
        return await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.Workspace.Id == workspace.Id)
            .Where(entry => entry.User.Id == user.Id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<ICollection<TimeEntryEntity>> GetActiveEntriesAsync(WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.Workspace.Id == workspace.Id)
            .ToListAsync();
    }

    public async Task<TimeEntryEntity?> GetActiveEntryForPastDay(
        ISession? session = null,
        CancellationToken cancellationToken = default
    )
    {
        session = session ?? _sessionProvider.CurrentSession;
        return await session.Query<TimeEntryEntity>()
            .FirstOrDefaultAsync(
                entry => entry.EndTime == null && entry.Date < DateTime.UtcNow,
                cancellationToken: cancellationToken
            );
    }
}
