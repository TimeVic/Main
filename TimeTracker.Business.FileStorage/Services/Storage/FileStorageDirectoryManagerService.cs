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

    #region Create
    
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
    
    #endregion
    
    #region Get
    
    public ICollection<FileStorageDirectoryEntity> GetTreeBranchByPath(FileStorageBucketEntity bucket, string? path)
    {
        var directoriesTree = bucket.DirectoriesTree;
        var directoriesToFind = PathToArray(path);
        if (directoriesToFind.Length == 0)
        {
            return directoriesTree;
        }
        return GetTreeBranchByPathRecursive(bucket.DirectoriesTree, directoriesToFind);
    }
    
    private ICollection<FileStorageDirectoryEntity> GetTreeBranchByPathRecursive(
        ICollection<FileStorageDirectoryEntity> currentTreeBranch,
        string[] directories
    )
    {
        var directoryToFind = directories.FirstOrDefault();
        if (string.IsNullOrEmpty(directoryToFind))
        {
            return currentTreeBranch;
        }
        var parentDir = currentTreeBranch.FirstOrDefault(item => item.Name == directoryToFind);
        if (parentDir == null)
        {
            return new List<FileStorageDirectoryEntity>();
        }

        return GetTreeBranchByPathRecursive(parentDir.Children, directories.Skip(1).ToArray());
    }
    
    public ICollection<FileStorageDirectoryEntity> GetTreeBranchAsListByPath(FileStorageBucketEntity bucket, string? path)
    {
        var resultList = new List<FileStorageDirectoryEntity>();
        var pathDirectories = GetTreeBranchByPath(bucket, path);
        foreach (var directory in pathDirectories)
        {
            resultList.Add(directory);
            resultList = resultList.Concat(
                GetTreeBranchAsListByPathRecursive(directory.Children)    
            ).ToList();
        }
        return resultList;
    }
    
    private ICollection<FileStorageDirectoryEntity> GetTreeBranchAsListByPathRecursive(
        ICollection<FileStorageDirectoryEntity> directories
    )
    {
        var actualList = new List<FileStorageDirectoryEntity>();
        foreach (var directory in directories)
        {
            actualList.Add(directory);
            var children = GetTreeBranchAsListByPathRecursive(directory.Children);
            actualList = actualList.Concat(children).ToList();
        }
        return actualList;
    }
    
    #endregion

    private string[] PathToArray(string? path)
    {
        path = path?.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return Array.Empty<string>();
        }

        var trimmedPath = path.Replace("\\", "/").TrimLastSlash().RemoveLeadingSlash();
        return (trimmedPath ?? string.Empty).Split('/')
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }
}
