﻿using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ITimeEntryDao: IDomainService
{
    Task<TimeEntryEntity?> GetByIdAsync(long? id);
    
    Task<TimeEntryEntity> StartNewAsync(
        WorkspaceEntity workspace,
        bool isBillable = false,
        string? description = null,
        long? projectId = null
    );
    
    Task<TimeEntryEntity?> StopActiveAsync(WorkspaceEntity workspace);

    Task<TimeEntryEntity> SetAsync(
        WorkspaceEntity workspace,
        TimeEntryCreationDto timeEntryDto,
        ProjectEntity? project = null
    );

    Task<bool> HasAccessAsync(UserEntity user, TimeEntryEntity? entity);

    Task<TimeEntryEntity?> GetActiveEntryAsync(WorkspaceEntity workspace);

    Task<ListDto<TimeEntryEntity>> GetListAsync(WorkspaceEntity workspace, int page);
}
