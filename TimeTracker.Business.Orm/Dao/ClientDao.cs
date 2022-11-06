using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class ClientDao: IClientDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public ClientDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ClientEntity> CreateAsync(WorkspaceEntity workspace, string name)
    {
        var entity = new ClientEntity()
        {
            Name = name,
            Workspace = workspace,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        workspace.Clients.Add(entity);
        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }

    public async Task<ClientEntity?> GetById(long? clientId, WorkspaceEntity? workspace = null)
    {
        if (clientId == null)
            return null;
        var query = _sessionProvider.CurrentSession.Query<ClientEntity>()
            .Where(item => item.Id == clientId);
        if (workspace != null)
        {
            query = query.Where(item => item.Workspace.Id == workspace.Id);
        }

        return await query.FirstOrDefaultAsync();
    }
    
    public async Task<ListDto<ClientEntity>> GetListAsync(WorkspaceEntity workspace, int page)
    {
        var query = _sessionProvider.CurrentSession.Query<ClientEntity>()
            .Where(item => item.Workspace.Id == workspace.Id);
        
        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query.Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .OrderByDescending(item => item.Name)
            .ToListAsync();
        return new ListDto<ClientEntity>(
            items,
            await query.CountAsync()
        );
    }
}
