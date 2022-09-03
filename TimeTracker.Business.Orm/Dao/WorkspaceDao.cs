﻿using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class WorkspaceDao: IWorkspaceDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public WorkspaceDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<WorkspaceEntity> CreateWorkspace(UserEntity user, string name)
    {
        var workspace = new WorkspaceEntity()
        {
            Name = name,
            User = user,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        user.Workspaces.Add(workspace);
        await _sessionProvider.CurrentSession.SaveAsync(user);
        return workspace;
    }
}
