using Autofac;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.FileStorage.FileStorageDirectoryManagerServiceTests;

public class CreateRecursiveTest: BaseTest
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageBucketSeeder _fileStorageBucketSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly FileStorageBucketEntity _bucket;
    private readonly IFileStorageDirectoryManagerService _directoryManagerService;

    public CreateRecursiveTest(): base()
    {
        _fileStorageService = Scope.Resolve<IFileStorageService>();
        _fileStorageBucketSeeder = Scope.Resolve<IFileStorageBucketSeeder>();
        _directoryManagerService = Scope.Resolve<IFileStorageDirectoryManagerService>();
        _userSeeder = Scope.Resolve<IUserSeeder>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _bucket = _fileStorageBucketSeeder.CreateAsync(_user).Result;
    }

    [Theory]
    [InlineData("/ ", "", 0)]
    [InlineData("/", "", 0)]
    [InlineData("2", "2", 1)]
    [InlineData("\\Program Files (x86)\\Windows Media Player\\en-US\\", "Program Files (x86)/Windows Media Player/en-US", 3)]
    [InlineData("/Windows Media Player\\en-US/", "Windows Media Player/en-US", 2)]
    [InlineData("/Windows Media Player/en-US\\", "Windows Media Player/en-US", 2)]
    public async Task ShouldCreate(string path, string expectedPath, int directoriesCount)
    {
        // Act
        var actualChildDirectory = await _directoryManagerService.CreateRecursive(_bucket, path);

        // Assert
        Assert.Equal(expectedPath, actualChildDirectory?.FullPath ?? string.Empty);
        Assert.Equal(directoriesCount, _bucket.Directories.Count);
    }
}
