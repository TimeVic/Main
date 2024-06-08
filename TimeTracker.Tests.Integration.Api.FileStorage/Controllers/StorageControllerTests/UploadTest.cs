using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.FileStorage;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Api.FileStorage.Core;

namespace TimeTracker.Tests.Integration.Api.FileStorage.Controllers.StorageControllerTests;

public class UploadTest: BaseTest
{
    private readonly string Url = "/storage/upload";
    
    private readonly UserEntity _user;
    private readonly IFileStorageBucketSeeder _fileStorageBucketSeeder;
    private readonly FileStorageAccessKeyEntity _accessKey;

    public UploadTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _fileStorageBucketSeeder = ServiceProvider.GetRequiredService<IFileStorageBucketSeeder>();
        var fileStorageAccessKeyDao = ServiceProvider.GetRequiredService<IFileStorageAccessKeyDao>();
        
        _user = UserSeeder.CreateActivatedAsync().Result;
        _accessKey = fileStorageAccessKeyDao.Create(_user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        // Arrange
        await DbSessionProvider.CurrentSession.DeleteAsync(_accessKey);
        
        var bucket = await _fileStorageBucketSeeder.CreateAsync(_user);
        var fileToUpload = CreateFormFile("image.jpg");
        
        // Act
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            accessKey: _accessKey,
            data: new Dictionary<string, object>
            {
                { "Bucket", bucket.Name },
                { "Directory", "" },
            },
            file: fileToUpload
        );
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpload()
    {
        var bucket = await _fileStorageBucketSeeder.CreateAsync(_user);
        
        var fileToUpload = CreateFormFile("image.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            accessKey: _accessKey,
            data: new Dictionary<string, object>
            {
                { "Bucket", bucket.Name },
                { "Directory", "" },
            },
            file: fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<FileStorageFileDto>();
        Assert.NotEmpty(actualData!.Id);
        Assert.NotEmpty(actualData.PublicUrl);
        Assert.NotEmpty(actualData.FileName);
        Assert.Null(actualData.Directory);
    }
    
    [Fact]
    public async Task ShouldUploadWithDirectory()
    {
        // Arrange
        var bucket = await _fileStorageBucketSeeder.CreateAsync(_user);
        var fileToUpload = CreateFormFile("image.jpg");
        
        // Act
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            accessKey: _accessKey,
            data: new Dictionary<string, object>
            {
                { "Bucket", bucket.Name },
                { "Directory", "/Some Directory 1/Some Directory 2" },
            },
            file: fileToUpload
        );
        response.EnsureSuccessStatusCode();

        // Assert
        var actualData = await response.GetJsonDataAsync<FileStorageFileDto>();
        Assert.Equal("Some Directory 1/Some Directory 2", actualData.Directory);
    }
    
    [Fact]
    public async Task ShouldUploadIfHasNoAccessToBucket()
    {
        var user2 = await UserSeeder.CreateActivatedAsync();
        var bucket = await _fileStorageBucketSeeder.CreateAsync(user2);
        
        var fileToUpload = CreateFormFile("image.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            accessKey: _accessKey,
            data: new Dictionary<string, object>
            {
                { "Bucket", bucket.Name },
                { "Directory", "" },
            },
            file: fileToUpload
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
