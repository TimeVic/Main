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

        if (IsImageMimeType(mimeType))
        {
            if (!await ImageHelper.IsImage(fileData))
            {
                throw new IncorrectFileException("Provided file content is not image");
            }
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

        try
        {
            await UploadCroppedImagesAsync(storedFile, fileData, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private async Task UploadCroppedImagesAsync(
        StoredFileEntity storedFile,
        byte[] fileData,
        CancellationToken cancellationToken
    )
    {
        var uploadTasks = Enum.GetValues<StorageImageSize>()
            .Select(imageSize => UploadCroppedImageAsync(storedFile, fileData, imageSize, cancellationToken))
            .ToArray();

        var croppedImages = await Task.WhenAll(uploadTasks);
        var defaultThumbPath = croppedImages
            .FirstOrDefault(item => item.Size == StorageImageSize.S_256 && item.IsUploaded)
            .FilePath;
        if (!string.IsNullOrEmpty(defaultThumbPath))
        {
            storedFile.ThumbCloudFilePath = defaultThumbPath;
        }
    }

    private async Task<(StorageImageSize Size, string FilePath, bool IsUploaded)> UploadCroppedImageAsync(
        StoredFileEntity storedFile,
        byte[] fileData,
        StorageImageSize imageSize,
        CancellationToken cancellationToken
    )
    {
        var cloudFilePath = GetCroppedImageFilePath(storedFile, imageSize);
        try
        {
            using var sourceStream = new MemoryStream(fileData);
            using var croppedImage = await ImageHelper.ResizeImageFromStreamAsync(
                sourceStream,
                (int)imageSize,
                (int)imageSize,
                ResizeMode.Crop,
                isGrayscale: false
            );
            using var croppedStream = new MemoryStream();
            await croppedImage.SaveAsPngAsync(croppedStream, cancellationToken: cancellationToken);
            croppedStream.PrepareToCopy();

            _logger.LogDebug($"S3 cropped image uploading started: {cloudFilePath}");
            var cloudResponse = await _storageClient.Upload(
                cloudFilePath,
                croppedStream,
                cancellationToken: cancellationToken
            );
            if (cloudResponse == null)
            {
                return (imageSize, cloudFilePath, false);
            }

            _logger.LogDebug($"S3 cropped image uploading finished: {cloudFilePath}");
            return (imageSize, cloudFilePath, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return (imageSize, cloudFilePath, false);
        }
    }

    private string GetCroppedImageFilePath(StoredFileEntity storedFile, StorageImageSize imageSize)
    {
        var directory = Path.GetDirectoryName(storedFile.CloudFilePath)?.Replace("\\", "/");
        var fileName = Path.GetFileNameWithoutExtension(storedFile.CloudFilePath);
        var croppedFileName = $"{fileName}_{(int)imageSize}.png";
        return string.IsNullOrEmpty(directory)
            ? croppedFileName
            : $"{directory}/{croppedFileName}";
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
