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

    public FileStorageService(
        IMongoClient mongoClient,
        IDbSessionProvider dbSessionProvider
    )
    {
        _mongoClient = mongoClient;
        _dbSessionProvider = dbSessionProvider;
    }

    public async Task<FileStorageFileEntity> Put(FileStorageBucketEntity bucket, IFormFile file)
    {
        var fileEntity = new FileStorageFileEntity()
        {
            ExternalId = SecurityUtil.GetTimeBasedToken(),
            Bucket = bucket,
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
            "",
            file.InternalFilePath,
            fileStream
        );
        file.Size = fileStream.Length;
        file.MongoId = mongoId.ToString();
        await _dbSessionProvider.CurrentSession.SaveAsync(file);
        return file;
    }
}
