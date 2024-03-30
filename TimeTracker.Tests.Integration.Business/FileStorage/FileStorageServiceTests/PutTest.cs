using Autofac;
using TimeTracker.Business.FileStorage.Services;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.FileStorage.FileStorageServiceTests;

public class PutTest: BaseTest
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageBucketSeeder _fileStorageBucketSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly FileStorageBucketEntity _bucket;

    public PutTest(): base()
    {
        _fileStorageService = Scope.Resolve<IFileStorageService>();
        _fileStorageBucketSeeder = Scope.Resolve<IFileStorageBucketSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _bucket = _fileStorageBucketSeeder.CreateAsync(_user).Result;
    }

    [Fact]
    public async Task ShouldPutNewFile()
    {
        // Arrange
        var file = CreateFormFile();
        
        // Act
        var actualFile = await _fileStorageService.Put(_bucket, file);

        // Assert
        Assert.NotEmpty(actualFile.ExternalId);
        Assert.NotEmpty(actualFile.MongoId);
        Assert.NotEmpty(actualFile.InternalFilePath);
        Assert.Equal("application/pdf", actualFile.MimeType);
        Assert.Equal("test.pdf", actualFile.OriginalFileName);
        Assert.Equal("pdf", actualFile.Extension);
        Assert.Equal(file.Length, actualFile.Size);
        Assert.Equal(_user, actualFile.Bucket!.User);
    }
}
