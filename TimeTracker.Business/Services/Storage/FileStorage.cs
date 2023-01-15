using Domain.Abstractions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using SixLabors.ImageSharp;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    private const int MaxFileSize = 1024 * 1024 * 15; // 15Mb
    private const int Thumb_MaxWidth = 256;
    private const int Thumb_MaxHeight = 256;
    
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly ILogger<IFileStorage> _logger;
    private const string CredentialsFilepath = "../../../../.credentials/google.json";
    
    private readonly StorageClient _googleClient;
    private readonly Bucket _bucket;
    private readonly string? _bucketName;
    private readonly string? _projectId;

    // https://storage.cloud.google.com/timevic-development/attachment/2023/1/917b6a7a-157a-4b47-a3ec-29363e9503cc.pdf
    
    public FileStorage(
        IConfiguration configuration,
        IDbSessionProvider dbSessionProvider,
        ILogger<IFileStorage> logger
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _logger = logger;
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

        if (IsImageMimeType(mimeType))
        {
            try
            {
                fileStream.Position = 0;
                var thumbImage = await ImageHelper.ResizeImageFromStreamAsync(
                    fileStream,
                    Thumb_MaxWidth,
                    Thumb_MaxHeight
                );
                using var thumbStream = new MemoryStream();
                await thumbImage.SaveAsPngAsync(thumbStream);
                var cloudThumbFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath("png")}";
                var cloudThumbFile = await _googleClient.UploadObjectAsync(
                    _bucketName,
                    cloudThumbFileName,
                    "image/png",
                    thumbStream
                );
                if (cloudFile != null)
                {
                    storedFile.ThumbCloudFilePath = cloudThumbFile.Name;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        await _dbSessionProvider.CurrentSession.SaveAsync(storedFile);
        await fileStream.DisposeAsync();
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
        if (file.Length > MaxFileSize)
        {
            throw new IncorrectFileException($"File can not be large than {(MaxFileSize / 1024 / 1024)}Mb");
        }

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

    private bool IsImageMimeType(string mimeType)
    {
        switch (mimeType)
        {
            case "image/png":
            case "image/jpeg":
            case "image/gif":
            case "image/bmp":
            case "image/webp":
                return true;
        }

        return false;
    }
}
