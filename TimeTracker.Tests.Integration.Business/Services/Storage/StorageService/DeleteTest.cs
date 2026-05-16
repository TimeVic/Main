using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class DeleteTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;

    public DeleteTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task ShouldDeleteForTask()
    {
        var file = await _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment);

        await FlushDbChanges(true);
        await _fileStorage.DeleteFile(_user, file.Id);
        
        await FlushDbChanges(true);
        var actualFile = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(file.Id);
        Assert.Null(actualFile);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfNashNoAccess()
    {
        var task2 = await _taskSeeder.CreateAsync();
        var file = await _fileStorage.PutFileAsync(task2, CreateFormFile(), StoredFileType.Attachment);

        await FlushDbChanges(true);
        await Assert.ThrowsAsync<HasNoAccessException>(async () =>
        {
            await _fileStorage.DeleteFile(_user, file.Id);
        });
    }
    
}
