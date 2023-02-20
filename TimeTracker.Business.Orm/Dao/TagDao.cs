using System.Drawing;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class TagDao: ITagDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TagDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }
    
    public async Task<TagEntity> CreateAsync(
        WorkspaceEntity workspace,
        string name,
        Color? color = null
    )
    {
        var tag = new TagEntity()
        {
            Name = name,
            Color = color,
            Workspace = workspace,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        workspace.Tags.Add(tag);
        await _sessionProvider.CurrentSession.SaveAsync(tag);
        return tag;
    }
    
    public async Task<TagEntity?> GetById(long? id)
    {
        if (id == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<TagEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<ICollection<TagEntity>> GetList(WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<TagEntity>()
            .Where(item => item.Workspace.Id == workspace.Id)
            .ToListAsync();
    }
}
