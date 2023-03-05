using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task<StoredFileEntity?> UploadFirstPendingToCloud(CancellationToken cancellationToken = default)
    {
        var fileToUpload = await _storedFilesDao.GetFirstToUpload();
        if (fileToUpload == null)
        {
            return null;
        }

        try
        {
            using var fileStream = new MemoryStream();
            fileStream.Write(fileToUpload.DataToUpload);

            var s3Request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileToUpload.CloudFilePath,
                InputStream = fileStream,
                AutoCloseStream = false,
                StreamTransferProgress = (sender, args) =>
                {
                    _logger.LogTrace($"S3 file uploading progress: {args.PercentDone}%");
                }
            };
            
            _logger.LogDebug($"S3 file uploading started: {fileToUpload.CloudFilePath}");
            var cloudFile = await _s3Client.PutObjectAsync(s3Request, cancellationToken);
            if (cloudFile == null)
            {
                throw new Exception($"File was not uploaded to cloud: {fileToUpload.CloudFilePath}");
            }
            _logger.LogDebug($"S3 file uploading finished: {fileToUpload.CloudFilePath}");

            if (IsImageMimeType(fileToUpload.MimeType))
            {
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
                    var cloudThumbFileName = $"{GetParentDir(fileToUpload)}/{fileToUpload.Type.GetFilePath("png")}";
                    
                    _logger.LogDebug($"S3 file thumb uploading started: {cloudThumbFileName}");
                    var cloudThumbResponse = await _s3Client.PutObjectAsync(
                        new PutObjectRequest()
                        {
                            BucketName = _bucketName,
                            Key = cloudThumbFileName,
                            InputStream = thumbStream
                        },
                        cancellationToken: cancellationToken
                    );
                    if (cloudThumbResponse != null)
                    {
                        _logger.LogDebug($"S3 file thumb uploading finished: {cloudThumbFileName}");
                        fileToUpload.ThumbCloudFilePath = cloudThumbFileName;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }

            fileToUpload.Status = StoredFileStatus.Uploaded;
            fileToUpload.DataToUpload = null;
            fileToUpload.UploadingError = null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            fileToUpload.Status = StoredFileStatus.Error;
            fileToUpload.UploadingError = e.Message;
        }
        finally
        {
            await _dbSessionProvider.CurrentSession.SaveAsync(fileToUpload, cancellationToken);
        }

        return fileToUpload;
    }
}
