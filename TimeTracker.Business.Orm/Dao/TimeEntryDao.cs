using System.ComponentModel.DataAnnotations;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Exceptions;

namespace TimeTracker.Business.Orm.Dao;

public class TimeEntryDao: ITimeEntryDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TimeEntryDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
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
    
    public async Task<ListDto<TimeEntryEntity>> GetListAsync(WorkspaceEntity workspace, int page)
    {
        var query = _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(item => item.Workspace.Id == workspace.Id);
        
        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query.Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ToListAsync();
        return new ListDto<TimeEntryEntity>(
            items,
            await query.CountAsync()
        );
    }
    
    public async Task<TimeEntryEntity> StartNewAsync(
        WorkspaceEntity workspace,
        bool isBillable = false,
        string? description = "",
        long? projectId = null
    )
    {
        await StopActiveAsync(workspace);
        
        var entry = new TimeEntryEntity
        {
            IsBillable = isBillable,
            Description = description,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            Workspace = workspace,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        if (projectId != null)
        {
            entry.Project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        }
        await _sessionProvider.CurrentSession.SaveAsync(entry);

        return entry;
    }

    public async Task<TimeEntryEntity?> StopActiveAsync(WorkspaceEntity workspace)
    {
        var activeTimeEntry = await GetActiveEntryAsync(workspace);
        if (activeTimeEntry != null)
        {
            activeTimeEntry.EndTime = DateTime.UtcNow;
            await _sessionProvider.CurrentSession.SaveAsync(activeTimeEntry);
        }
        return activeTimeEntry;
    }
    
    public async Task<TimeEntryEntity> SetAsync(
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
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };
        }
        timeEntry.Project = project;
        timeEntry.Description = timeEntryDto.Description;
        timeEntry.HourlyRate = timeEntryDto.HourlyRate;
        timeEntry.IsBillable = timeEntryDto.IsBillable;
        timeEntry.StartTime = timeEntryDto.StartTime;
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
                .Where(item => item.User.Id == user.Id)
                .And(() => timeEntryAlias.Id == entity.Id)
            .RowCountAsync();
        return itemsWithAccessCount > 0;
    }
    
    public async Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(entry => entry.EndTime == null)
            .Where(entry => entry.Workspace.Id == workspace.Id)
            .FirstOrDefaultAsync();
    }
}
