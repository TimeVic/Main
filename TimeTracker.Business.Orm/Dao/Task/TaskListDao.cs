﻿using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public class TaskListDao: ITaskListDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TaskListDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<TaskListEntity> CreateList(ProjectEntity project, string name)
    {
        var taskList = new TaskListEntity()
        {
            Name = name,
            Project = project,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };
        await _sessionProvider.CurrentSession.SaveAsync(taskList);
        return taskList;
    }
}
