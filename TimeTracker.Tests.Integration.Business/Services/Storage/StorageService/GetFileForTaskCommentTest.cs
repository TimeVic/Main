using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class GetFileForTaskCommentTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskCommentEntity _taskComment;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;
    private readonly ITaskCommentSeeder _taskCommentSeeder;
    private readonly TaskEntity _task;

    public GetFileForTaskCommentTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _user = _userSeeder.CreateActivatedAsync().Result;
        _taskCommentSeeder = Scope.Resolve<ITaskCommentSeeder>();

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        _taskComment = _taskCommentSeeder.CreateAsync(_task).Result;
    }

    [Fact]
    public async Task ShouldGetFile()
    {
        var actualFile = await _fileStorage.PutFileAsync(_taskComment, CreateFormFile(), StoredFileType.Attachment);
        await _fileStorage.UploadFirstPendingToCloud();

        await CommitDbChanges();
        var (expectedFile, fileStream) = await _fileStorage.GetFileStream(_user, actualFile.Id);

        Assert.True(fileStream.Length > 0);
    }
}
