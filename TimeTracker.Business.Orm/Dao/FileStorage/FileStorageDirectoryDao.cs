using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.Orm.Dao.FileStorage;

public class FileStorageDirectoryDao: IFileStorageDirectoryDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public FileStorageDirectoryDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<FileStorageDirectoryEntity> CreateOrUpdate(
        FileStorageBucketEntity bucket,
        string name,
        FileStorageDirectoryEntity? parent = null
    )
    {
        var directory = bucket.Directories.FirstOrDefault(item => item.Name == name);
        if (directory == null)
        {
            directory = new FileStorageDirectoryEntity()
            {
                Name = name,
                Bucket = bucket,
                CreateTime = DateTime.UtcNow
            };
            bucket.Directories.Add(directory);
            parent?.Children.Add(directory);
        }

        directory.Parent = parent;
        directory.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(directory);
        return directory;
    }
}
