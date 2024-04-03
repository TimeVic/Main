using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public class FileStorageBucketDao: IFileStorageBucketDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageBucketDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<FileStorageBucketEntity?> GetBy(long? id = null, string? name = null)
    {
        return await _sessionProvider.CurrentSession.Query<FileStorageBucketEntity>()
            .Where(item => item.Id == id || item.Name == name)
            .FirstOrDefaultAsync();
    }
    
    public async Task<FileStorageBucketEntity?> GetByName(UserEntity user, string name)
    {
        return await _sessionProvider.CurrentSession.Query<FileStorageBucketEntity>()
            .Where(item => item.User == user && item.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<FileStorageBucketEntity> Create(UserEntity user, string name)
    {
        var entity = new FileStorageBucketEntity()
        {
            Name = name,
            User = user,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }
}
