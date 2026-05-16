using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Storage.StorageService;

public class UploadToCloudTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;

    public UploadToCloudTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _user = _userSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldUploadToCloud()
    {
        var uploadedFile = await _fileStorage.PutFileAsync(_user, CreateFormFile(), StoredFileType.Attachment);

        Assert.NotNull(uploadedFile);
    }

    [Fact]
    public async Task ShouldUploadAndCreateThumbIfImage()
    {
        var formFile = CreateFormFile("images/image.jpg");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        Assert.NotEmpty(actualFile.ThumbCloudFilePath!);
    }
    
    [Fact]
    public async Task ShouldUploadImage()
    {
        var formFile = CreateFormFile("images/image.jpg");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        Assert.NotEqual(Guid.Empty, actualFile.Id);
    }
}
