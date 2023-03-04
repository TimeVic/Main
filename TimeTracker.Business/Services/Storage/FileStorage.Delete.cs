using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task DeleteFile(UserEntity user, long fileId)
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
        if (file.Status == StoredFileStatus.Pending)
        {
            throw new RecordCanNotBeModifiedException();
        }

        await _s3Client.DeleteObjectAsync(_bucketName, file.CloudFilePath);
        
        file.Tasks.Clear();
        await _dbSessionProvider.CurrentSession.DeleteAsync(file);
    }
}
