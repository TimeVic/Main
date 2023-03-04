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

public class UploadToCloudTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;

    public UploadToCloudTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
    }

    [Fact]
    public async Task ShouldUploadToCloud()
    {
        await _fileStorage.PutFileAsync(_user, CreateFormFile(), StoredFileType.Attachment);

        var uploadedFile = await _fileStorage.UploadFirstPendingToCloud();
        Assert.Null(uploadedFile.DataToUpload);
        Assert.Null(uploadedFile.UploadingError);
        Assert.Equal(StoredFileStatus.Uploaded, uploadedFile.Status);
    }

    [Fact]
    public async Task ShouldUploadAndCreateThumbIfImage()
    {
        var formFile = CreateFormFile("images/image.jpg");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        await CommitDbChanges();
        Assert.Null(actualFile.ThumbCloudFilePath);
        
        var uploadedFile = await _fileStorage.UploadFirstPendingToCloud();
        Assert.Null(uploadedFile.DataToUpload);
        Assert.Null(uploadedFile.UploadingError);
        Assert.Equal(StoredFileStatus.Uploaded, uploadedFile.Status);
        Assert.NotEmpty(uploadedFile.ThumbCloudFilePath);
    }
    
    [Fact]
    public async Task ShouldUploadImage()
    {
        var formFile = CreateFormFile("images/image.jpg");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
        
        var uploadedFile = await _fileStorage.UploadFirstPendingToCloud();
        Assert.Null(uploadedFile.DataToUpload);
        Assert.Null(uploadedFile.UploadingError);
        Assert.Equal(StoredFileStatus.Uploaded, uploadedFile.Status);
    }
    
    [Fact]
    public async Task ShouldNotUploadIfPendingNotFound()
    {
        var uploadedFile = await _fileStorage.UploadFirstPendingToCloud();
        Assert.Null(uploadedFile);
    }
}
