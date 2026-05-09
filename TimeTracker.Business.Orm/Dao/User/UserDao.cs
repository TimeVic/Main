using Autofac;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserDao: BaseDao, IUserDao
{
    public UserDao(ILifetimeScope scope): base(scope)
    {
    }

    public async Task<UserEntity?> GetExistsByUserName(string userName)
    {
        return await Session.Query<UserEntity>()
            .Where(item => item.UserName == userName.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetByEmail(string email)
    {
        return await Session.Query<UserEntity>()
            .Where(item => item.Email == email.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetById(Guid id)
    {
        return await Session.Query<UserEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetByVerificationToken(string token)
    {
        return await Session.Query<UserEntity>()
            .Where(item => item.VerificationToken == token)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity> CreatePendingUser(string email)
    {
        var user = new UserEntity
        {
            Email = email.Trim().ToLower(),
            VerificationToken = SecurityUtil.GetRandomString(32),
            VerificationTime = null,
            PasswordHash = [],
            PasswordSalt = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Timezone = TimeZoneInfo.Utc.Id
        };
        await Session.SaveAsync(user);
        return user;
    }
    
    public async Task<WorkspaceEntity?> GetUsersWorkspace(UserEntity user, Guid? workspaceId)
    {
        var allWorkspaces = await GetUsersWorkspaces(user);
        return workspaceId == null
            ? allWorkspaces.FirstOrDefault(item => item.IsDefault)
            : allWorkspaces.FirstOrDefault(item => item.Id == workspaceId);
    }
    
    public async Task<WorkspaceEntity> GetDefaultWorkspace(UserEntity user)
    {
        var allWorkspaces = await GetUsersWorkspaces(user);
        return allWorkspaces.First(item => item.IsDefault);
    }
    
    public async Task<ICollection<WorkspaceEntity>> GetUsersWorkspaces(UserEntity user, MembershipAccessType? accessType = null)
    {
        var query = Session.Query<WorkspaceMemberEntity>()
            .Fetch(item => item.Workspace)
            .Where(item => item.User.Id == user.Id);
        if (accessType != null)
        {
            query = query.Where(item => item.Access == accessType);
        }

        return await query.Select(item => item.Workspace)
            .ToListAsync();;
    }

    public async Task<UserEntity?> GetLastDemoUserAsync()
    {
        return await Session.Query<UserEntity>()
            .Where(item => item.Email.StartsWith("demo+") && item.Email.EndsWith("@timevic.com"))
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
