using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public class FileStorageAccessKeyDao: IFileStorageAccessKeyDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageAccessKeyDao(
        IDbSessionProvider sessionProvider
    )
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<FileStorageAccessKeyEntity> Create(UserEntity user, DateTime? expirationTime = null)
    {
        var entity = new FileStorageAccessKeyEntity()
        {
            User = user,
            AccessKey = SecurityUtil.GeneratePassword(12),
            SecretKey = SecurityUtil.GeneratePassword(32),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
            ExpirationTime = expirationTime
        };
        await _sessionProvider.CurrentSession.SaveAsync(entity);
        return entity;
    }

    public async Task<FileStorageAccessKeyEntity?> GetByKey(string accessKey, string secretKey)
    {
        return await _sessionProvider.CurrentSession.Query<FileStorageAccessKeyEntity>()
            .Where(item => item.AccessKey == accessKey && item.SecretKey == secretKey)
            .FirstOrDefaultAsync();
    }
}
