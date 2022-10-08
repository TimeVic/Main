using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IUserDao: IDomainService
{
    Task<UserEntity?> GetExistsByUserName(string userName);
    
    Task<UserEntity?> GetByEmail(string email);

    Task<UserEntity?> GetById(long id);
    
    Task<UserEntity> CreatePendingUser(string email);

    Task<UserEntity?> GetByVerificationToken(string token);

    Task<WorkspaceEntity?> GetUsersWorkspace(UserEntity user, long workspaceId);

    Task<ICollection<WorkspaceEntity>> GetUsersWorkspaces(UserEntity user);
}
