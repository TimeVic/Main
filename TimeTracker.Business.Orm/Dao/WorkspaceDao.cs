using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public class WorkspaceDao: BaseDao, IWorkspaceDao
{
    private readonly ICurrencyDao _currencyDao;

    public WorkspaceDao(
        ILifetimeScope scope,
        ICurrencyDao currencyDao
    ): base(scope)
    {
        _currencyDao = currencyDao;
    }

    public async Task<WorkspaceEntity?> GetById(Guid? id)
    {
        if (id == null)
            return null;

        return await Session.Query<WorkspaceEntity>()
            .FirstOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);
    }

    public async Task<IReadOnlyCollection<WorkspaceEntity>> GetDeletedBeforeAsync(DateTime deletedBefore)
    {
        return await Session.Query<WorkspaceEntity>()
            .Where(item => item.DeletedAt != null && item.DeletedAt <= deletedBefore)
            .OrderBy(item => item.DeletedAt)
            .ToListAsync();
    }

    public async Task<WorkspaceEntity?> GetDeletedByIdAsync(Guid id)
    {
        return await Session.Query<WorkspaceEntity>()
            .FirstOrDefaultAsync(item => item.Id == id && item.DeletedAt != null);
    }

    public async Task<int> GetActiveCreatedWorkspacesCountAsync(UserEntity user)
    {
        return await Session.Query<WorkspaceEntity>()
            .Where(item => item.CreatedUser.Id == user.Id)
            .Where(item => item.IsDefault == false && item.DeletedAt == null)
            .CountAsync();
    }
    
    public async Task<WorkspaceEntity> CreateWorkspaceAsync(UserEntity user, string name, bool isDefault = false)
    {
        var workspace = new WorkspaceEntity()
        {
            Name = name,
            CreatedUser = user,
            Currency = await _currencyDao.GetDefault(),
            TimeZone = GlobalConstants.DefaultTimeZone,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.CreatedWorkspaces.Add(workspace);
        await Session.SaveAsync(workspace);
        return workspace;
    }
    
    public async Task<WorkspaceEntity> SetModeAsync(WorkspaceEntity workspace, WorkspaceMode mode)
    {
        workspace.Mode = mode;
        workspace.UpdatedAt = DateTime.UtcNow;
        await Session.SaveOrUpdateAsync(workspace);
        return workspace;
    }

    public async Task<WorkspaceEntity> UpdateWorkspaceAsync(
        WorkspaceEntity workspace,
        string name,
        CurrencyEntity currency,
        string timeZone,
        string? description
    )
    {
        workspace.Name = name;
        workspace.Currency = currency;
        workspace.TimeZone = timeZone;
        workspace.Description = description;
        workspace.UpdatedAt = DateTime.UtcNow;
        await Session.SaveOrUpdateAsync(workspace);
        return workspace;
    }
    
    public async Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace)
    {
        return await Session.Query<TimeEntryEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.EndTime == null)
            .AnyAsync();
    }
    
    public async Task<ListDto<WorkspaceMemberEntity>> GetMembersAsync(WorkspaceEntity workspace, int page)
    {
        var query = Session.Query<WorkspaceMemberEntity>()
            .Where(item => item.Workspace.Id == workspace.Id);
        
        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query
            .Fetch(item => item.User)
            .ThenFetch(item => item.Language)
            .OrderByDescending(item => item.Id)
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ToListAsync();
        return new ListDto<WorkspaceMemberEntity>(
            items,
            await query.CountAsync()
        );
    }
    
    public async Task<WorkspaceMemberEntity> GetMemberAsync(Guid id)
    {
        return await Session.Query<WorkspaceMemberEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
}
