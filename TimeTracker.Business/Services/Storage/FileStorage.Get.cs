using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task<(StoredFileEntity, Stream)> GetFileStream(UserEntity user, Guid fileId)
    {
        var (file, fileStream, _) = await GetFileStream(user, fileId, imageSize: null);
        return (file, fileStream);
    }

    public async Task<(StoredFileEntity File, Stream FileStream, string MimeType)> GetFileStream(
        UserEntity user,
        Guid fileId,
        StorageImageSize? imageSize
    )
    {
        var file = await _dbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(fileId);
        if (file == null)
        {
            throw new RecordNotFoundException();
        }
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, file.Relationship))
        {
            throw new HasNoAccessException();
        }

        if (imageSize.HasValue)
        {
            // SVG files are vector images and do not require raster resizing; return the original file stream
            if (string.Equals(file.MimeType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            {
                var svgStream = await _storageClient.GetAsStream(file.CloudFilePath);
                return (file, svgStream, file.MimeType);
            }

            if (!IsImageMimeType(file.MimeType))
            {
                throw new IncorrectFileException("Image size can be requested only for image files");
            }

            var isDefaultImageSize = imageSize.Value == StorageImageSize.S_256;
            var imageFilePath = isDefaultImageSize && !string.IsNullOrEmpty(file.ThumbCloudFilePath)
                ? file.ThumbCloudFilePath
                : GetCroppedImageFilePath(file, imageSize.Value);
            var croppedFileStream = await _storageClient.GetAsStream(imageFilePath);
            return (file, croppedFileStream, "image/png");
        }

        var fileStream = await _storageClient.GetAsStream(file.CloudFilePath);
        return (file, fileStream, file.MimeType);
    }
}
