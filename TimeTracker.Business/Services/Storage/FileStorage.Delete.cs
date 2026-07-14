using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task DeleteFile(UserEntity user, Guid fileId)
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
        await _storageClient.Delete(file.CloudFilePath);
        
        file.Tasks.Clear();
        file.Users.Clear();
        file.TaskComments.Clear();
        file.NoteNodes.Clear();
        await _dbSessionProvider.CurrentSession.DeleteAsync(file);
    }
}
