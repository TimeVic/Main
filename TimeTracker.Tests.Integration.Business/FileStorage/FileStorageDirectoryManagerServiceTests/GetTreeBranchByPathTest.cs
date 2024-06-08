using Autofac;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.FileStorage.FileStorageDirectoryManagerServiceTests;

public class GetTreeBranchByPathTest: BaseTest
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageBucketSeeder _fileStorageBucketSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly FileStorageBucketEntity _bucket;
    private readonly IFileStorageDirectoryManagerService _directoryManagerService;

    public GetTreeBranchByPathTest(): base()
    {
        _fileStorageService = Scope.Resolve<IFileStorageService>();
        _fileStorageBucketSeeder = Scope.Resolve<IFileStorageBucketSeeder>();
        _directoryManagerService = Scope.Resolve<IFileStorageDirectoryManagerService>();
        _userSeeder = Scope.Resolve<IUserSeeder>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _bucket = _fileStorageBucketSeeder.CreateAsync(_user).Result;
    }

    [Fact]
    public async Task ShouldGetAllTreeIfPathEmpty()
    {
        // Arrange
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 1/Test Sub Dir1");
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 1/Test Sub Dir2");
        await _directoryManagerService.CreateRecursive(_bucket, " Test dir 2/Test Sub Dir3");
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 3/Test Sub Dir3");
        
        // Act
        var actualTree = _directoryManagerService.GetTreeBranchByPath(_bucket, "");

        // Assert
        Assert.Equal(3, actualTree.Count);
        Assert.Contains(actualTree, item => item.Name == "Test dir 1");
        Assert.Contains(actualTree, item => item.Name == "Test dir 2");
        Assert.Contains(actualTree, item => item.Name == "Test dir 3");
    }
    
    [Fact]
    public async Task ShouldGetChildDirectoryIfProvided()
    {
        // Arrange
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 1/Test Sub Dir1");
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 1/Test Sub Dir2/Test Sub Dir4/Test Sub Dir5");
        await _directoryManagerService.CreateRecursive(_bucket, "Test dir 1/Test Sub Dir2/Test Sub Dir4/Test Sub Dir6/Test Sub Dir7");
        
        // Act
        var actualTree = _directoryManagerService.GetTreeBranchByPath(_bucket, "Test dir 1/Test Sub Dir2/Test Sub Dir4");

        // Assert
        Assert.Equal(2, actualTree.Count);
        Assert.Contains(actualTree, item => item.Name == "Test Sub Dir5");
        Assert.Contains(actualTree, item => item.Name == "Test Sub Dir6");
    }
}
