using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Dao.FileStorage;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.FileStorage.Core;

namespace TimeTracker.Tests.Integration.Api.FileStorage.Controllers.FileControllerTests;

public class GetTest: BaseTest
{
    private readonly string Url = "/file/";
    
    private readonly UserEntity _user;
    private readonly IFileStorageBucketSeeder _fileStorageBucketSeeder;
    private readonly FileStorageAccessKeyEntity _accessKey;
    private readonly IFileStorageService _fileStorageService;

    public GetTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _fileStorageBucketSeeder = ServiceProvider.GetRequiredService<IFileStorageBucketSeeder>();
        _fileStorageService = ServiceProvider.GetRequiredService<IFileStorageService>();
        var fileStorageAccessKeyDao = ServiceProvider.GetRequiredService<IFileStorageAccessKeyDao>();
        
        _user = UserSeeder.CreateActivatedAsync().Result;
        _accessKey = fileStorageAccessKeyDao.Create(_user).Result;
    }

    [Fact]
    public async Task ShouldGetAsUnauthorized()
    {
        // Arrange
        var bucket = await _fileStorageBucketSeeder.CreateAsync(_user);
        var fileToUpload = CreateFormFile("image.jpg");
        var uploadedFile = await _fileStorageService.Put(bucket, fileToUpload);
        
        // Act
        var response = await GetRequestAsync(
            string.Format($"{Url}{bucket.Name}/{uploadedFile.ExternalId}"),
            accessKey: null
        );
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(fileContent);
    }
    
    [Fact]
    public async Task ShouldGet()
    {
        // Arrange
        var bucket = await _fileStorageBucketSeeder.CreateAsync(_user);
        var fileToUpload = CreateFormFile("image.jpg");
        var uploadedFile = await _fileStorageService.Put(bucket, fileToUpload);
        
        // Act
        var response = await GetRequestAsync(
            string.Format($"{Url}{bucket.Name}/{uploadedFile.ExternalId}"),
            accessKey: _accessKey
        );
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(fileContent);
    }
}
