using System.Text.RegularExpressions;
using Autofac;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.User;

public class UserDao: BaseDao, IUserDao
{
    private readonly TimeTracker.Business.Orm.Dao.ILanguageDao _languageDao;

    public UserDao(
        ILifetimeScope scope,
        TimeTracker.Business.Orm.Dao.ILanguageDao languageDao
    ): base(scope)
    {
        _languageDao = languageDao;
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

    public async Task<UserEntity?> GetByLogin(string login)
    {
        var cleanLogin = StringUtils.NormalizeLogin(login);
        return await Session.Query<UserEntity>()
            .Where(item => item.Login == cleanLogin)
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetByLoginOrEmail(string loginOrEmail)
    {
        var clean = loginOrEmail.Trim().ToLower();
        if (clean.Contains('@'))
        {
            return await GetByEmail(clean);
        }
        var cleanLogin = StringUtils.NormalizeLogin(clean);
        return await Session.Query<UserEntity>()
            .Where(item => item.Login == cleanLogin || item.Email == clean)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<UserEntity>> FindByLogin(string query, int take = 10)
    {
        var cleanQuery = StringUtils.NormalizeLogin(query);
        if (string.IsNullOrWhiteSpace(cleanQuery))
        {
            return new List<UserEntity>();
        }
        return await Session.Query<UserEntity>()
            .Where(item => item.Login != null && item.Login.Contains(cleanQuery))
            .Take(take)
            .ToListAsync();
    }

    public async Task<string> GenerateUniqueLogin(string email)
    {
        var emailPrefix = StringUtils.GetUserNameFromEmail(email) ?? "user";
        var cleanLogin = Regex.Replace(emailPrefix.ToLower(), @"[^a-z0-9]+", "_").Trim('_');
        if (string.IsNullOrEmpty(cleanLogin))
        {
            cleanLogin = "user";
        }
        else if (cleanLogin.Length < 3)
        {
            cleanLogin = cleanLogin.PadRight(3, '0');
        }

        var candidate = cleanLogin;
        var suffix = 1;
        while (await IsLoginExistsAsync(candidate))
        {
            candidate = $"{cleanLogin}_{suffix}";
            suffix++;
        }
        return candidate;
    }

    public async Task<bool> IsLoginExistsAsync(string login, Guid? excludeUserId = null)
    {
        var cleanLogin = StringUtils.NormalizeLogin(login);
        var query = Session.Query<UserEntity>()
            .Where(item => item.Login == cleanLogin);
        if (excludeUserId.HasValue)
        {
            query = query.Where(item => item.Id != excludeUserId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<UserEntity> ChangeLoginAsync(UserEntity user, string newLogin)
    {
        var cleanLogin = StringUtils.NormalizeLogin(newLogin);
        if (await IsLoginExistsAsync(cleanLogin, user.Id))
        {
            throw new RecordIsExistsException("User with this login already exists");
        }
        user.Login = cleanLogin;
        user.UpdatedAt = DateTime.UtcNow;
        await Session.SaveAsync(user);
        return user;
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
        var login = await GenerateUniqueLogin(email);
        var user = new UserEntity
        {
            Email = email.Trim().ToLower(),
            Login = login,
            VerificationToken = SecurityUtil.GetRandomString(32),
            VerificationTime = null,
            PasswordHash = [],
            PasswordSalt = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Timezone = TimeZoneInfo.Utc.Id,
            Language = await _languageDao.GetDefaultAsync()
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

    public async Task<WorkspaceEntity> GetSelectedWorkspaceAsync(UserEntity user)
    {
        var allWorkspaces = await GetUsersWorkspaces(user);
        var selectedWorkspaceId = user.SelectedWorkspace?.Id;
        if (selectedWorkspaceId.HasValue)
        {
            var selectedWorkspace = allWorkspaces.FirstOrDefault(item => item.Id == selectedWorkspaceId.Value);
            if (selectedWorkspace != null)
            {
                return selectedWorkspace;
            }
        }

        return allWorkspaces.First(item => item.IsDefault);
    }

    public async Task<UserEntity> SelectWorkspaceAsync(UserEntity user, WorkspaceEntity workspace)
    {
        user.SelectedWorkspace = workspace;
        user.UpdatedAt = DateTime.UtcNow;
        await Session.SaveAsync(user);
        return user;
    }

    public async Task<UserEntity> UpdateSettingsAsync(UserEntity user, string? userName, LanguageEntity language)
    {
        user.UserName = string.IsNullOrWhiteSpace(userName) ? null : userName.Trim();
        user.Language = language;
        user.UpdatedAt = DateTime.UtcNow;
        await Session.SaveAsync(user);
        return user;
    }
    
    public async Task<ICollection<WorkspaceEntity>> GetUsersWorkspaces(UserEntity user, MembershipAccessType? accessType = null)
    {
        var query = Session.Query<WorkspaceMemberEntity>()
            .Fetch(item => item.Workspace)
            .ThenFetch(item => item.Currency)
            .Fetch(item => item.Workspace)
            .ThenFetch(item => item.CreatedUser)
            .Where(item => item.User.Id == user.Id)
            .Where(item => item.Workspace.DeletedAt == null);
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
