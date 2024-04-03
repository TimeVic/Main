using Microsoft.AspNetCore.Http;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public class FileStorageService: IFileStorageService
{
    private readonly IMongoClient _mongoClient;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IFileStorageDirectoryManagerService _directoryManagerService;

    public FileStorageService(
        IMongoClient mongoClient,
        IDbSessionProvider dbSessionProvider,
        IFileStorageDirectoryManagerService directoryManagerService
    )
    {
        _mongoClient = mongoClient;
        _dbSessionProvider = dbSessionProvider;
        _directoryManagerService = directoryManagerService;
    }

    public async Task<FileStorageFileEntity> Put(
        FileStorageBucketEntity bucket,
        IFormFile file,
        string? directoryPath = null
    )
    {
        var directory = await _directoryManagerService.CreateRecursive(bucket, directoryPath);
        var fileEntity = new FileStorageFileEntity()
        {
            MongoId = string.Empty,
            ExternalId = SecurityUtil.GetTimeBasedToken(),
            Bucket = bucket,
            Directory = directory,
            OriginalFileName = file.FileName,
            Name = file.Name,
            Extension = file.GetExtension(),
            MimeType = MimeTypeHelper.GetMimeTypeByName(file.FileName),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        await _dbSessionProvider.CurrentSession.SaveAsync(fileEntity);

        return await Put(fileEntity, file.OpenReadStream());
    }
    
    public async Task<FileStorageFileEntity> Put(FileStorageFileEntity file, Stream fileStream)
    {
        var mongoId = await _mongoClient.UploadFileFromStream(
            file.Bucket!.Name,
            file.InternalFileName,
            fileStream
        );
        file.Size = fileStream.Length;
        file.MongoId = mongoId.ToString();
        await _dbSessionProvider.CurrentSession.SaveAsync(file);
        return file;
    }
    
    public Task<Stream> DownloadToStream(FileStorageFileEntity file)
    {
        return _mongoClient.DownloadToStream(
            file.Bucket!.Name,
            file.InternalFileName
        );
    }
}
