using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.FileStorage;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public class FileStorageDirectoryManagerService: IFileStorageDirectoryManagerService
{
    private readonly IFileStorageDirectoryDao _directoryDao;

    public FileStorageDirectoryManagerService(
        IFileStorageDirectoryDao directoryDao    
    )
    {
        _directoryDao = directoryDao;
    }

    public async Task<FileStorageDirectoryEntity?> CreateRecursive(FileStorageBucketEntity bucket, string? path)
    {
        var directories = PathToArray(path);
        if (directories.Length == 0)
        {
            return null;
        }
        return await CreateRecursive(bucket, directories, 0);
    }

    private async Task<FileStorageDirectoryEntity> CreateRecursive(
        FileStorageBucketEntity bucket,
        string[] directories, 
        int index,
        FileStorageDirectoryEntity? parent = null
    )
    {
        var directoryName = directories[index];
        var childDirectory = await _directoryDao.CreateOrUpdate(bucket, directoryName, parent);
        var nextIndex = ++index;
        if (index >= directories.Length)
        {
            return childDirectory;
        }
        return await CreateRecursive(bucket, directories, nextIndex, childDirectory);
    }

    private string[] PathToArray(string? path)
    {
        path = path?.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return Array.Empty<string>();
        }
        return path.Replace("\\", "/")
            .TrimLastSlash()
            .RemoveLeadingSlash()
            .Split('/')
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }
}
