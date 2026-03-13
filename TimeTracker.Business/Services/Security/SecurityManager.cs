using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;

    public SecurityManager(
        IWorkspaceAccessService workspaceAccessService,
        IUserDao userDao
    )
    {
        _workspaceAccessService = workspaceAccessService;
        _userDao = userDao;
    }

    public async Task CheckAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity)
    {
        if (entity == null)
            throw new RecordNotFoundException();
        if (!await HasAccess(accessLevel, user, entity))
        {
            throw new HasNoAccessException();
        }
    }

    public async Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity)
    {
        if (entity == null)
            return false;
        
        if (entity is WorkspaceEntity workspaceEntity)
        {
            // This validation is used to perform basic actions in
            // the workplace. Such as adding clients, projects, and so on.
            return await HasAccessToWorkspace(accessLevel, user, workspaceEntity);
        }
        if (entity is TimeEntryEntity entryEntity)
        {
            return await HasAccessToTimeEntry(accessLevel, user, entryEntity);
        }
        if (entity is ProjectEntity projectEntity)
        {
            return await HasAccessToProject(accessLevel, user, projectEntity);
        }
        if (entity is ClientEntity clientEntity)
        {
            return await HasAccessToClientAsync(accessLevel, user, clientEntity);
        }
        if (entity is PaymentEntity paymentEntity)
        {
            return await HasAccessToPayment(accessLevel, user, paymentEntity);
        }
        if (entity is TaskEntity taskEntity)
        {
            return await HasAccessToTask(user, taskEntity);
        }
        if (entity is TaskListEntity taskList)
        {
            return await HasAccessToTaskList(user, taskList);
        }
        if (entity is TaskCommentEntity taskCommentEntity)
        {
            return await HasAccessToTaskComment(accessLevel, user, taskCommentEntity);
        }
        if (entity is GoalsTrackerEntity goalsTrackerEntity)
        {
            return await HasAccessToGoalsTracker(accessLevel, user, goalsTrackerEntity);
        }
        if (entity is NotificationEntity notificationEntity)
        {
            return await HasAccessToNotification(accessLevel, user, notificationEntity);
        }

        throw new NotImplementedException($"Security checking not implemented for {entity?.GetTypeName()}");
    }

    private async Task<bool> HasAccessToWorkspace(AccessLevel accessLevel, UserEntity user, WorkspaceEntity workspace)
    {
        var usersWorkspaces = await _userDao.GetUsersWorkspace(user, workspace.Id);
        if (usersWorkspaces == null)
        {
            // The user does not belong to any workplace
            return false;
        }

        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        return (
            accessLevel == AccessLevel.Write
            && accessType
                is MembershipAccessType.Owner
                or MembershipAccessType.Manager
        ) || (
            accessLevel == AccessLevel.Read
            && accessType
                is MembershipAccessType.Owner
                or MembershipAccessType.Manager
                or MembershipAccessType.User
        );
    }

    private async Task<bool> HasAccessToTimeEntry(AccessLevel accessLevel, UserEntity user, TimeEntryEntity timeEntry)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, timeEntry.Workspace);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Write 
                && accessType == MembershipAccessType.User
                && timeEntry.User.Id == user.Id
            )
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToClientAsync(AccessLevel accessLevel, UserEntity user, ClientEntity client)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, client.Workspace);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToProject(AccessLevel accessLevel, UserEntity user, ProjectEntity project)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, project);
        return accessType == MembershipAccessType.Owner 
            || accessType == MembershipAccessType.Manager
            || (
                accessLevel == AccessLevel.Read 
                && accessType == MembershipAccessType.User
            );
    }
    
    private async Task<bool> HasAccessToPayment(AccessLevel accessLevel, UserEntity user, PaymentEntity payment)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, payment.Workspace);
        return accessType != null
            && payment.User.Id == user.Id;
    }
    
    private async Task<bool> HasAccessToTask(UserEntity user, TaskEntity task)
    {
        return await HasAccessToProject(AccessLevel.Read, user, task.TaskList.Project);
    }
    
    private async Task<bool> HasAccessToTaskComment(AccessLevel accessLevel, UserEntity user, TaskCommentEntity taskComment)
    {
        var hasAccessToTask = await HasAccessToProject(
            AccessLevel.Read,
            user,
            taskComment.Task.TaskList.Project
        );
        if (!hasAccessToTask)
        {
            return false;
        }
        if (
            accessLevel == AccessLevel.Read
            || (
                accessLevel == AccessLevel.Write
                && taskComment.User!.Id == user.Id
            )
        )
        {
            return true;
        }
        return false;
    }
    
    private async Task<bool> HasAccessToTaskList(UserEntity user, TaskListEntity taskList)
    {
        return await HasAccessToProject(AccessLevel.Read, user, taskList.Project);
    }
    
    private async Task<bool> HasAccessToGoalsTracker(AccessLevel accessLevel, UserEntity user, GoalsTrackerEntity goalsTrackerEntity)
    {
        var hasAccessToWorkspace = await HasAccessToWorkspace(
            AccessLevel.Read,
            user,
            goalsTrackerEntity.Workspace
        );
        if (!hasAccessToWorkspace)
        {
            return false;
        }
        return goalsTrackerEntity.User.Id == user.Id;
    }
    
    private async Task<bool> HasAccessToNotification(AccessLevel accessLevel, UserEntity user, NotificationEntity notificationEntity)
    {
        var hasAccessToWorkspace = await HasAccessToWorkspace(
            AccessLevel.Read,
            user,
            notificationEntity.Workspace
        );
        if (!hasAccessToWorkspace)
        {
            return false;
        }
        return notificationEntity.ReceiverUser.Id == user.Id;
    }
}
