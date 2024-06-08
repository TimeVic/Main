using Autofac;
using TimeTracker.Business.FileStorage.Services;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.FileStorage.MongoClientTests;

public class UploadFromStreamTest: BaseTest
{
    private readonly IMongoClient _mongoClient;

    public UploadFromStreamTest(): base()
    {
        _mongoClient = Scope.Resolve<IMongoClient>();
    }

    [Fact]
    public async Task ShouldUpload()
    {
        var file = CreateFormFile();
        var fileId = await _mongoClient.UploadFileFromStream("test_bucket", file.FileName, file.OpenReadStream());
        Assert.NotEmpty(fileId.ToString());
        Assert.True(fileId.CreationTime.ToUniversalTime() >= DateTime.MinValue);
    }
}
