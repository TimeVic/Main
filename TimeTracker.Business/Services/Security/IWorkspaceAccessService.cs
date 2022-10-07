﻿using Domain.Abstractions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Services.Security;

public interface IWorkspaceAccessService: IDomainService
{
    public Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectEntity>? projects = null
    );

    Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        string email,
        MembershipAccessType access,
        ICollection<ProjectEntity>? projects = null
    );

    Task<bool> RemoveAccessAsync(WorkspaceEntity workspace, UserEntity user);
    
    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        WorkspaceEntity entryWorkspace,
        ProjectEntity? project = null
    );

    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        ProjectEntity project
    );
}