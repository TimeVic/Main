using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task<(StoredFileEntity, Stream)> GetFileStream(UserEntity user, long fileId)
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

        var fileResponse = await _s3Client.GetObjectAsync(_bucketName, file.CloudFilePath);
        if (fileResponse == null)
        {
            throw new RecordNotFoundException($"S3 File not found: {file.CloudFilePath}");
        }

        var fileStream = new MemoryStream();
        await fileResponse.ResponseStream.CopyToAsync(fileStream);
        return (file, fileStream);
    }
}
