using System.Net;
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
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Dao;
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
    
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly ILogger<IFileStorage> _logger;
    private readonly IFileStorageRelationshipService _relationshipService;
    private readonly ISecurityManager _securityManager;
    private readonly IStoredFilesDao _storedFilesDao;
    private readonly IFileStorageClient _storageClient;
    
    public FileStorage(
        IConfiguration configuration,
        IDbSessionProvider dbSessionProvider,
        ILogger<IFileStorage> logger,
        IFileStorageRelationshipService relationshipService,
        ISecurityManager securityManager,
        IStoredFilesDao storedFilesDao,
        IFileStorageS3Client storageS3Client,
        IFileStorageGoogleClient storageGoogleClient
    )
    {
        _dbSessionProvider = dbSessionProvider;
        _logger = logger;
        _relationshipService = relationshipService;
        _securityManager = securityManager;
        _storedFilesDao = storedFilesDao;
        
        // Google Client selected by default
        _storageClient = storageGoogleClient;
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
        var cloudFileName = $"{GetParentDir(entity)}/{fileType.GetFilePath(fileExtension)}";

        if (IsImageMimeType(mimeType))
        {
            if (!await ImageHelper.IsImage(fileData))
            {
                throw new IncorrectFileException("Provided file content is not image");
            }
        }

        var storedFile = new StoredFileEntity()
        {
            Extension = fileExtension,
            MimeType = mimeType,
            CloudFilePath = cloudFileName,
            OriginalFileName = fileName,
            Type = fileType,
            Size = fileData.Length,
            DataToUpload = fileData,
            Status = StoredFileStatus.Pending,
            CreateTime = DateTime.UtcNow
        };

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
