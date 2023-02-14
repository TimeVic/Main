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

public class PutFileForTaskTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    
    private readonly TaskEntity _task;

    public PutFileForTaskTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();

        _task = _taskSeeder.CreateAsync().Result;
    }

    [Fact]
    public async Task ShouldPutFile()
    {
        var actualFile = await _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
        Assert.NotEmpty(actualFile.MimeType);
        Assert.NotEmpty(actualFile.CloudFilePath);
        Assert.NotNull(actualFile.Extension);
        Assert.NotEmpty(actualFile.OriginalFileName);
        Assert.True(actualFile.Size > 0);
        Assert.Equal(StoredFileType.Attachment, actualFile.Type);

        await CommitDbChanges();
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        Assert.Contains(actualTask.Attachments, item => item.Id == actualFile.Id);
    }
}
