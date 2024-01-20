using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface INotificationDao: IDomainService
{
    Task<int> GetCount(UserEntity user, WorkspaceEntity workspace, bool isUnread = false);

    Task<int> MarkAllAsRead(UserEntity user, WorkspaceEntity workspace);
}
