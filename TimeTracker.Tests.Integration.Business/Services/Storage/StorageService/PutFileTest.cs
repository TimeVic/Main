using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class PutFileTest: BaseTest
{
    private readonly IFileStorage _fileStorage;

    public PutFileTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
    }

    [Fact]
    public async Task ShouldPutFile()
    {
        var actualFile = await _fileStorage.PutFileAsync(new UserEntity(), CreateFormFile(), StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
        Assert.NotEmpty(actualFile.MimeType);
        Assert.NotEmpty(actualFile.CloudFilePath);
        Assert.NotNull(actualFile.Extension);
        Assert.NotEmpty(actualFile.OriginalFileName);
        Assert.True(actualFile.Size > 0);
        Assert.Equal(StoredFileType.Attachment, actualFile.Type);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNoExtension()
    {
        var fileWithoutExtension = CreateFormFile("test");
        await Assert.ThrowsAsync<IncorrectFileException>(async () =>
        {
            await _fileStorage.PutFileAsync(new UserEntity(), fileWithoutExtension, StoredFileType.Attachment);
        });
    }
}
