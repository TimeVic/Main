using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.User;

public interface IUserDao: IDomainService
{
    Task<UserEntity?> GetExistsByUserName(string userName);
    
    Task<UserEntity?> GetByEmail(string email);

    Task<string?> GetLanguageCodeByEmailAsync(string email);

    Task<UserEntity?> GetById(Guid id);
    
    Task<UserEntity> CreatePendingUser(string email);

    Task<UserEntity?> GetByVerificationToken(string token);

    Task<WorkspaceEntity?> GetUsersWorkspace(UserEntity user, Guid? workspaceId);

    Task<ICollection<WorkspaceEntity>> GetUsersWorkspaces(UserEntity user, MembershipAccessType? accessType = null);

    Task<WorkspaceEntity> GetDefaultWorkspace(UserEntity user);

    Task<WorkspaceEntity> GetSelectedWorkspaceAsync(UserEntity user);

    Task<UserEntity> SelectWorkspaceAsync(UserEntity user, WorkspaceEntity workspace);

    Task<UserEntity> UpdateSettingsAsync(UserEntity user, string? userName, LanguageEntity language);

    Task<UserEntity?> GetLastDemoUserAsync();
}
