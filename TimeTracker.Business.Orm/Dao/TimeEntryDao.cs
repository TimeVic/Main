using System.ComponentModel.DataAnnotations;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
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

    public async Task<TimeEntryEntity> StartNewAsync(
        WorkspaceEntity workspace,
        bool isBillable,
        string description = "",
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
        var activeTimeEntry = await workspace.TimeEntries.AsQueryable()
            .Where(entry => entry.EndTime == null)
            .FirstOrDefaultAsync();
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
            throw new ValidationException("EndTime can not be less than StartTime");
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
        timeEntry.EndTime = timeEntryDto.EndTime;

        await _sessionProvider.CurrentSession.SaveAsync(timeEntry);
        return timeEntry;
    }
}
