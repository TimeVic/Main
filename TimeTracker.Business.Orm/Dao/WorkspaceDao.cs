using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Dao;

public class WorkspaceDao: IWorkspaceDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public WorkspaceDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<WorkspaceEntity> CreateWorkspaceAsync(UserEntity user, string name, bool isDefault = false)
    {
        var workspace = new WorkspaceEntity()
        {
            Name = name,
            User = user,
            IsDefault = isDefault,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        user.Workspaces.Add(workspace);
        await _sessionProvider.CurrentSession.SaveAsync(user);
        return workspace;
    }
    
    public async Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .Where(item => item.EndTime == null)
            .AnyAsync();
    }
    
    public async Task<ListDto<WorkspaceMembershipEntity>> GetMembershipsAsync(WorkspaceEntity workspace, int page)
    {
        var query = _sessionProvider.CurrentSession.Query<WorkspaceMembershipEntity>()
            .Where(item => item.Workspace.Id == workspace.Id);
        
        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ToListAsync();
        return new ListDto<WorkspaceMembershipEntity>(
            items,
            await query.CountAsync()
        );
    }
    
    public async Task<WorkspaceMembershipEntity> GetMembershipAsync(long id)
    {
        return await _sessionProvider.CurrentSession.Query<WorkspaceMembershipEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
}
