using Domain.Abstractions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public class FileStorage: IFileStorage
{
    private readonly IDbSessionProvider _dbSessionProvider;
    private const string CredentialsFilepath = "../../../../.credentials/google.json";
    
    private readonly StorageClient _googleClient;
    private readonly Bucket _bucket;
    private readonly string? _bucketName;
    private readonly string? _projectId;

    // https://storage.cloud.google.com/timevic-development/attachment/2023/1/917b6a7a-157a-4b47-a3ec-29363e9503cc.pdf
    
    public FileStorage(
        IConfiguration configuration,
        IDbSessionProvider dbSessionProvider
    )
    {
        _dbSessionProvider = dbSessionProvider;
        var filePath = Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Google Cloud credentials file not found: {filePath}");
        }

        using var credentialsStream = new FileStream(
            Path.Combine(AssemblyUtils.GetAssemblyPath(), CredentialsFilepath),
            FileMode.Open,
            FileAccess.Read
        );
        var credentials = GoogleCredential.FromStream(credentialsStream);
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));
        _bucketName = configuration.GetValue<string>("Google:Storage:BucketName");
        if (_bucketName == null)
            throw new ArgumentNullException(nameof(_bucketName));
        _projectId = configuration.GetValue<string>("Google:Storage:ProjectId");
        if (_projectId == null)
            throw new ArgumentNullException(nameof(_projectId));

        credentials.CreateScoped();
        _googleClient = StorageClient.Create(credentials);
    }

    public async Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        Stream fileStream,
        string fileName,
        StoredFileType fileType
    ) where TEntity : IEntity
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var cloudFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath(fileExtension)}";
        var mimeType = MimeTypeHelper.GetMimeType(fileExtension);
        
        var cloudFile = await _googleClient.UploadObjectAsync(_bucketName, cloudFileName, mimeType, fileStream);
        if (cloudFile == null)
        {
            throw new Exception($"File was not uploaded to cloud: {cloudFileName}");
        }
        var storedFile = new StoredFileEntity()
        {
            Extension = fileExtension,
            MimeType = mimeType,
            CloudFilePath = cloudFile.Name,
            OriginalFileName = fileName,
            Type = fileType,
            Size = Convert.ToInt64(cloudFile.Size),
            CreateTime = DateTime.UtcNow
        };
        await _dbSessionProvider.CurrentSession.SaveAsync(storedFile);
        return storedFile;
    }

    public async Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        StoredFileType fileType
    ) where TEntity: IEntity
    {
        ValidateFileType(formFile, fileType);
        using var fileStream = new MemoryStream();
        await formFile.CopyToAsync(fileStream);
        return await PutFileAsync(entity, fileStream, formFile.FileName, fileType);
    }
    
    private void ValidateFileType(IFormFile file, StoredFileType fileType)
    {
        var mimeType = MimeTypeHelper.GetMimeType(file.GetExtension());
        if (string.IsNullOrEmpty(mimeType))
        {
            throw new IncorrectFileException("Incorrect file extension");
        }
        var isValidMimeType = fileType.GetAllowedMimeTypes().Any(item => item == mimeType);
        if (!isValidMimeType)
        {
            throw new IncorrectFileException("Incorrect file type");
        }
    }

    private string GetParentDir<TEntity>(TEntity entity) where TEntity: IEntity
    {
        if (entity is UserEntity)
        {
            return "user";
        }

        return "common";
    }
}
