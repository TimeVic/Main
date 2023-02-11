﻿using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public interface ITaskDao: IDomainService
{
    Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? notificationTime = null
    );
}