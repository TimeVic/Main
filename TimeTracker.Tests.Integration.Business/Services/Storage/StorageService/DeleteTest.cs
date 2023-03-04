using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class DeleteTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;

    public DeleteTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task ShouldDeleteForTask()
    {
        var file = await _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment);
        await _fileStorage.UploadFirstPendingToCloud();

        await CommitDbChanges();
        await _fileStorage.DeleteFile(_user, file.Id);
        await CommitDbChanges();

        var actualFile = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(file.Id);
        Assert.Null(actualFile);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfNashNoAccess()
    {
        var task2 = _taskSeeder.CreateAsync().Result;
        var file = await _fileStorage.PutFileAsync(task2, CreateFormFile(), StoredFileType.Attachment);

        await Assert.ThrowsAsync<HasNoAccessException>(async () =>
        {
            await _fileStorage.DeleteFile(_user, file.Id);
        });
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfPendingStatus()
    {
        var file = await _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment);
        await CommitDbChanges();

        await Assert.ThrowsAsync<RecordCanNotBeModifiedException>(async () =>
        {
            await _fileStorage.DeleteFile(_user, file.Id);
        });
    }
}
