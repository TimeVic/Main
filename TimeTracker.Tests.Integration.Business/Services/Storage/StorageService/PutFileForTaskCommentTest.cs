using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class PutFileForTaskCommentTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskCommentSeeder _taskCommentSeeder;
    
    private readonly TaskCommentEntity _taskComment;

    public PutFileForTaskCommentTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _taskCommentSeeder = Scope.Resolve<ITaskCommentSeeder>();

        _taskComment = _taskCommentSeeder.CreateAsync().Result;
    }

    [Fact]
    public async Task ShouldPutFile()
    {
        var actualFile = await _fileStorage.PutFileAsync(_taskComment, CreateFormFile(), StoredFileType.Attachment);
        Assert.NotEqual(Guid.Empty, actualFile.Id);
        Assert.NotEmpty(actualFile.MimeType);
        Assert.NotEmpty(actualFile.CloudFilePath);
        Assert.NotNull(actualFile.Extension);
        Assert.NotEmpty(actualFile.OriginalFileName);
        Assert.True(actualFile.Size > 0);
        Assert.Equal(StoredFileType.Attachment, actualFile.Type);

        await CommitDbChanges();
        var actualTaskComment = await DbSessionProvider.CurrentSession.GetAsync<TaskCommentEntity>(_taskComment.Id);
        Assert.Contains(actualTaskComment.Attachments, item => item.Id == actualFile.Id);
    }
}
