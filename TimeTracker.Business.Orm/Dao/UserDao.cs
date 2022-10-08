﻿using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Dao;

public class UserDao: IUserDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public UserDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<UserEntity?> GetExistsByUserName(string userName)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => item.UserName == userName.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetByEmail(string email)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => item.Email == email.Trim().ToLower())
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetById(long id)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<UserEntity?> GetByVerificationToken(string token)
    {
        return await _sessionProvider.CurrentSession.Query<UserEntity>()
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
            PasswordHash = new byte[] {},
            PasswordSalt = new byte[] {},
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(user);
        return user;
    }
    
    public async Task<WorkspaceEntity?> GetUsersWorkspace(UserEntity user, long workspaceId)
    {
        var allWorkspaces = await GetUsersWorkspaces(user);
        return allWorkspaces.FirstOrDefault(item => item.Id == workspaceId);
    }
    
    public async Task<ICollection<WorkspaceEntity>> GetUsersWorkspaces(UserEntity user)
    {
        var ownedWorkspaces = user.Workspaces;
        var otherWorkspaces = await _sessionProvider.CurrentSession.Query<WorkspaceMembershipEntity>()
            .Select(item => item.Workspace)
            .ToListAsync();
        return ownedWorkspaces.Concat(otherWorkspaces).ToList();
    }
}
