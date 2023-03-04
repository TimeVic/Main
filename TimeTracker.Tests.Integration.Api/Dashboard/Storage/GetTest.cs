using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Storage;

public class GetTest: BaseTest
{
    private readonly string Url = "/dashboard/storage/file/{0}";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly ITaskSeeder _taskSeeder;
    
    private readonly TaskEntity _task;
    private readonly IFileStorage _fileStorage;
    private readonly StoredFileEntity _uploadedFile;
    private readonly IStoredFilesDao _storedFilesDao;

    public GetTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _storedFilesDao = ServiceProvider.GetRequiredService<IStoredFilesDao>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        (_jwtToken, _user, var workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        
        _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment).Wait();
        _uploadedFile = _fileStorage.UploadFirstPendingToCloud().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await GetRequestAsAnonymousAsync(
            string.Format(Url, _uploadedFile.Id)
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDownloadFile()
    {
        var response = await GetRequestAsync(
            string.Format(Url, _uploadedFile.Id),
            _jwtToken
        );
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(fileContent);
    }
    
    [Fact]
    public async Task ShouldDownloadFileWithQueryToken()
    {
        var response = await GetRequestAsync(
            string.Format(Url, _uploadedFile.Id),
            null,
            new Dictionary<string, string>()
            {
                { "api_token", _jwtToken }
            }
        );
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(fileContent);
    }
    
    [Fact]
    public async Task ShouldDownloadIfHasNotAccessToEntity()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var task = _taskSeeder.CreateAsync(user: user2).Result;
        
        var response = await GetRequestAsync(
            string.Format(Url, _uploadedFile.Id),
            otherToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
