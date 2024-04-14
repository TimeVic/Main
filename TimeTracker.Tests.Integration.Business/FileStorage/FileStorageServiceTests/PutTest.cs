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
    private readonly IMongoClient _mongoClient;

    public PutTest(): base()
    {
        _fileStorageService = Scope.Resolve<IFileStorageService>();
        _fileStorageBucketSeeder = Scope.Resolve<IFileStorageBucketSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _mongoClient = Scope.Resolve<IMongoClient>();

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
        Assert.NotEmpty(actualFile.InternalFileName);
        Assert.Equal("application/pdf", actualFile.MimeType);
        Assert.Equal("test.pdf", actualFile.OriginalFileName);
        Assert.Equal("pdf", actualFile.Extension);
        Assert.Equal(file.Length, actualFile.Size);
        Assert.Equal(_user, actualFile.Bucket!.User);
    }
    
    [Fact]
    public async Task ShouldPutNewFileWithDirectory()
    {
        // Arrange
        var file = CreateFormFile();
        
        // Act
        var actualFile = await _fileStorageService.Put(_bucket, file, "some/Test Directory");

        // Assert
        Assert.Equal("application/pdf", actualFile.MimeType);
        Assert.Equal("Test Directory", actualFile.Directory!.Name);
    }
    
    [Fact]
    public async Task ShouldRemovePreviousFileWithSameFileNameWithoutDirectory()
    {
        // Arrange
        var fileName = "Test File.png";
        var file = CreateFormFile(fileName);
        var previousFile = await _fileStorageService.Put(_bucket, file);
        Assert.True(await _mongoClient.IsExists(previousFile.Bucket.Name, previousFile.InternalFileName));
        
        // Act
        file = CreateFormFile(fileName);
        var actualFile = await _fileStorageService.Put(_bucket, file);

        // Assert
        Assert.NotEqual(previousFile.Id, actualFile.Id);
        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<FileStorageFileEntity>(previousFile.Id));
        Assert.False(await _mongoClient.IsExists(previousFile.Bucket.Name, previousFile.InternalFileName));
    }
    
    [Fact]
    public async Task ShouldRemovePreviousFileWithSameFileNameInDirectory()
    {
        // Arrange
        var fileName = "Test File.png";
        var directory = "Test Directory1/Test Directory2";
        var otherDirectory = "Test Directory1/Test Directory3";
        
        var file = CreateFormFile(fileName);
        var previousFile = await _fileStorageService.Put(_bucket, file, directory);
        Assert.True(await _mongoClient.IsExists(previousFile.Bucket.Name, previousFile.InternalFileName));
        
        var otherFile = await _fileStorageService.Put(_bucket, file, otherDirectory);
        Assert.True(await _mongoClient.IsExists(previousFile.Bucket.Name, previousFile.InternalFileName));
        
        // Act
        file = CreateFormFile(fileName);
        var actualFile = await _fileStorageService.Put(_bucket, file, directory);

        // Assert
        Assert.NotEqual(previousFile.Id, actualFile.Id);
        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<FileStorageFileEntity>(previousFile.Id));
        Assert.False(await _mongoClient.IsExists(previousFile.Bucket.Name, previousFile.InternalFileName));
        Assert.True(await _mongoClient.IsExists(otherFile.Bucket.Name, otherFile.InternalFileName));
    }
}
