using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using SixLabors.ImageSharp;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    private const int MaxFileSize = 1024 * 1024 * 50; // 15Mb
    private const int Thumb_MaxWidth = 256;
    private const int Thumb_MaxHeight = 256;
    
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly ILogger<IFileStorage> _logger;
    private readonly IFileStorageRelationshipService _relationshipService;
    private readonly ISecurityManager _securityManager;
    
    private readonly string? _bucketName;
    private readonly AmazonS3Client _s3Client;
    
    public FileStorage(
        IConfiguration configuration,
        IDbSessionProvider dbSessionProvider,
        ILogger<IFileStorage> logger,
        IFileStorageRelationshipService relationshipService,
        ISecurityManager securityManager
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _logger = logger;
        _relationshipService = relationshipService;
        _securityManager = securityManager;
        
        var accessKey = configuration.GetValue<string>("AWS:S3:AccessKey");
        if (accessKey == null)
            throw new ArgumentNullException(nameof(accessKey));
        var secretKey = configuration.GetValue<string>("AWS:S3:SecretKey");
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));
        _bucketName = configuration.GetValue<string>("AWS:S3:BucketName");
        if (_bucketName == null)
            throw new ArgumentNullException(nameof(_bucketName));

        var options = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(options, Amazon.RegionEndpoint.EUCentral1);
    }

    public async Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        Stream fileStream,
        string fileName,
        StoredFileType fileType,
        CancellationToken cancellationToken = default
    ) where TEntity : IEntity
    {
        

        using var cloudFileStream = new MemoryStream();
        fileStream.PrepareToCopy();
        await fileStream.CopyToAsync(cloudFileStream, cancellationToken);
        cloudFileStream.PrepareToCopy();
        
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var cloudFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath(fileExtension)}";
        var mimeType = MimeTypeHelper.GetMimeType(fileExtension);

        var fileSize = fileStream.Length;
        var cloudFile = await _s3Client.PutObjectAsync(
            new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = cloudFileName,
                InputStream = cloudFileStream
            },
            cancellationToken: cancellationToken
        );
        if (cloudFile == null)
        {
            throw new Exception($"File was not uploaded to cloud: {cloudFileName}");
        }
        var storedFile = new StoredFileEntity()
        {
            Extension = fileExtension,
            MimeType = mimeType,
            CloudFilePath = cloudFileName,
            OriginalFileName = fileName,
            Type = fileType,
            Size = fileSize,
            CreateTime = DateTime.UtcNow
        };

        if (IsImageMimeType(mimeType))
        {
            using var cloudThumbFileStream = new MemoryStream();
            fileStream.PrepareToCopy();
            await fileStream.CopyToAsync(cloudThumbFileStream, cancellationToken);
            cloudThumbFileStream.PrepareToCopy();
            
            try
            {
                var thumbImage = await ImageHelper.ResizeImageFromStreamAsync(
                    cloudThumbFileStream,
                    Thumb_MaxWidth,
                    Thumb_MaxHeight
                );
                using var thumbStream = new MemoryStream();
                await thumbImage.SaveAsPngAsync(thumbStream, cancellationToken: cancellationToken);
                var cloudThumbFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath("png")}";
                await _s3Client.PutObjectAsync(
                    new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = cloudThumbFileName,
                        InputStream = thumbStream
                    },
                    cancellationToken: cancellationToken
                );
                if (cloudFile != null)
                {
                    storedFile.ThumbCloudFilePath = cloudThumbFileName;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        await _dbSessionProvider.CurrentSession.SaveAsync(storedFile, cancellationToken);
        await _relationshipService.AddFileRelationship(entity, storedFile);
        await fileStream.DisposeAsync();
        return storedFile;
    }

    public async Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        IFormFile formFile,
        StoredFileType fileType,
        CancellationToken cancellationToken = default
    ) where TEntity: IEntity
    {
        ValidateFileType(formFile, fileType);
        using var fileStream = new MemoryStream();
        await formFile.CopyToAsync(fileStream, cancellationToken);
        return await PutFileAsync(entity, fileStream, formFile.FileName, fileType, cancellationToken);
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
        if (entity is TaskEntity)
        {
            return "task";
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
