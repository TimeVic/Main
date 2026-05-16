using Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage.Client;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public const int MaxFileSize = 1024 * 1024 * 50; // 15Mb
    private const int Thumb_MaxWidth = 256;
    private const int Thumb_MaxHeight = 256;
    private const int Avatar_MaxWidth = 256;
    private const int Avatar_MaxHeight = 256;
    
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly ILogger<IFileStorage> _logger;
    private readonly IFileStorageRelationshipService _relationshipService;
    private readonly ISecurityManager _securityManager;
    private readonly IFileStorageGarageClient _storageClient;
    
    public FileStorage(
        IDbSessionProvider dbSessionProvider,
        ILogger<IFileStorage> logger,
        IFileStorageRelationshipService relationshipService,
        ISecurityManager securityManager,
        IFileStorageGarageClient storageGarageClient
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _logger = logger;
        _relationshipService = relationshipService;
        _securityManager = securityManager;
        
        // Garage client selected by default.
        _storageClient = storageGarageClient;
    }

    public async Task<StoredFileEntity> PutFileAsync<TEntity>(
        TEntity entity,
        byte[] fileData,
        string fileName,
        StoredFileType fileType,
        CancellationToken cancellationToken = default
    ) where TEntity : IEntity
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var mimeType = MimeTypeHelper.GetMimeTypeByExtension(fileExtension);
        if (fileType == StoredFileType.Avatar && !IsImageMimeType(mimeType))
        {
            throw new IncorrectFileException("Incorrect file type");
        }

        if (IsImageMimeType(mimeType))
        {
            if (!await ImageHelper.IsImage(fileData))
            {
                throw new IncorrectFileException("Provided file content is not image");
            }
        }

        if (fileType == StoredFileType.Avatar)
        {
            // Avatar uploads are cropped before cloud upload so profile images are stored in a consistent square size.
            fileData = await CropAvatarFileDataAsync(fileData, cancellationToken);
            fileExtension = "png";
            mimeType = "image/png";
        }

        var cloudFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath(fileExtension)}";

        var storedFile = new StoredFileEntity()
        {
            Extension = fileExtension,
            MimeType = mimeType,
            CloudFilePath = cloudFileName,
            OriginalFileName = fileName,
            Type = fileType,
            Size = fileData.Length,
            CreatedAt = DateTime.UtcNow
        };

        await UploadFileDataAsync(entity, storedFile, fileData, cancellationToken);
        await _dbSessionProvider.CurrentSession.SaveAsync(storedFile, cancellationToken);
        await _relationshipService.AddFileRelationship(entity, storedFile);
        return storedFile;
    }

    private async Task<byte[]> CropAvatarFileDataAsync(byte[] fileData, CancellationToken cancellationToken)
    {
        using var sourceStream = new MemoryStream(fileData);
        using var avatarImage = await ImageHelper.ResizeImageFromStreamAsync(
            sourceStream,
            Avatar_MaxWidth,
            Avatar_MaxHeight,
            ResizeMode.Crop,
            isGrayscale: false
        );
        using var avatarStream = new MemoryStream();
        await avatarImage.SaveAsPngAsync(avatarStream, cancellationToken: cancellationToken);
        return avatarStream.ToArray();
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
        return await PutFileAsync(entity, fileStream.ToArray(), formFile.FileName, fileType, cancellationToken);
    }
    
    private void ValidateFileType(IFormFile file, StoredFileType fileType)
    {
        if (file.Length > MaxFileSize)
        {
            throw new IncorrectFileException($"File can not be large than {(MaxFileSize / 1024 / 1024)}Mb");
        }

        var mimeType = MimeTypeHelper.GetMimeTypeByExtension(file.GetExtension());
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
        if (entity is TaskCommentEntity)
        {
            return "task_comment";
        }
        return "common";
    }

    private async Task UploadFileDataAsync<TEntity>(
        TEntity entity,
        StoredFileEntity storedFile,
        byte[] fileData,
        CancellationToken cancellationToken
    ) where TEntity: IEntity
    {
        using var fileStream = new MemoryStream(fileData);

        _logger.LogDebug($"S3 file uploading started: {storedFile.CloudFilePath}");
        var cloudFile = await _storageClient.Upload(storedFile.CloudFilePath, fileStream, cancellationToken);
        if (cloudFile == null)
        {
            throw new Exception($"File was not uploaded to cloud: {storedFile.CloudFilePath}");
        }
        _logger.LogDebug($"S3 file uploading finished: {storedFile.CloudFilePath}");

        if (!IsImageMimeType(storedFile.MimeType))
        {
            return;
        }

        fileStream.PrepareToCopy();
        try
        {
            var thumbImage = await ImageHelper.ResizeImageFromStreamAsync(
                fileStream,
                Thumb_MaxWidth,
                Thumb_MaxHeight
            );
            using var thumbStream = new MemoryStream();
            await thumbImage.SaveAsPngAsync(thumbStream, cancellationToken: cancellationToken);
            thumbStream.PrepareToCopy();
            var cloudThumbFileName = $"{GetParentDir(entity)}/{storedFile.Type.GetFilePath("png")}";

            _logger.LogDebug($"S3 file thumb uploading started: {cloudThumbFileName}");
            var cloudThumbResponse = await _storageClient.Upload(
                cloudThumbFileName,
                thumbStream,
                cancellationToken: cancellationToken
            );
            if (cloudThumbResponse != null)
            {
                _logger.LogDebug($"S3 file thumb uploading finished: {cloudThumbFileName}");
                storedFile.ThumbCloudFilePath = cloudThumbFileName;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
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
