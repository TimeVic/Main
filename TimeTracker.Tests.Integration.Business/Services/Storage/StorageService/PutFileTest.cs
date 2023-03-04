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

public class PutFileTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;

    public PutFileTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _user = _userSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldPutFile()
    {
        var actualFile = await _fileStorage.PutFileAsync(_user, CreateFormFile(), StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
        Assert.NotEmpty(actualFile.MimeType);
        Assert.NotEmpty(actualFile.CloudFilePath);
        Assert.NotNull(actualFile.Extension);
        Assert.NotEmpty(actualFile.OriginalFileName);
        Assert.NotNull(actualFile.DataToUpload);
        Assert.True(actualFile.Size > 0);
        Assert.Equal(StoredFileType.Attachment, actualFile.Type);
        Assert.Equal(StoredFileStatus.Pending, actualFile.Status);
    }
    
    [Fact]
    public async Task ShouldPutImage()
    {
        var formFile = CreateFormFile("images/image.jpg");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
    }
    
    [Fact]
    public async Task ShouldPutPdf()
    {
        var formFile = CreateFormFile("sample-30mb.pdf");
        var actualFile = await _fileStorage.PutFileAsync(_user, formFile, StoredFileType.Attachment);
        Assert.True(actualFile.Id > 0);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNoExtension()
    {
        var fileWithoutExtension = CreateFormFile("test");
        await Assert.ThrowsAsync<IncorrectFileException>(async () =>
        {
            await _fileStorage.PutFileAsync(_user, fileWithoutExtension, StoredFileType.Attachment);
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfFileDataIsNotImage()
    {
        var fileWithoutExtension = CreateFormFile("test.jpg", new byte[] { 1, 2 });
        await Assert.ThrowsAsync<IncorrectFileException>(async () =>
        {
            await _fileStorage.PutFileAsync(_user, fileWithoutExtension, StoredFileType.Attachment);
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfTooLarge()
    {
        var randomizer = new Random();
        var fileBytes = Enumerable
            .Repeat(0, 1024 * 1024 * 50 + 1)
            .Select(x => Convert.ToByte(randomizer.Next(0, 254)))
            .ToArray();
        var fileWithoutExtension = CreateFormFile("test.pdf", fileBytes);
        await Assert.ThrowsAsync<IncorrectFileException>(async () =>
        {
            await _fileStorage.PutFileAsync(_user, fileWithoutExtension, StoredFileType.Attachment);
        });
    }
}
