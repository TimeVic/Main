using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
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

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/storage/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly ITaskSeeder _taskSeeder;
    
    private readonly TaskEntity _task;
    private readonly IFileStorage _fileStorage;
    private readonly StoredFileEntity _uploadedFile;
    private readonly IStoredFilesDao _storedFilesDao;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var task = await _taskSeeder.CreateAsync();
        var response = await PostRequestAsAnonymousAsync(
            Url,
            new GetListRequest()
            {
                EntityId = task.Id,
                EntityType = StorageEntityType.Task
            }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGetForTask()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var actualFile1 = await _fileStorage.PutFileAsync(task, CreateFormFile(), StoredFileType.Attachment);
        var actualFile2 = await _fileStorage.PutFileAsync(task, CreateFormFile(), StoredFileType.Attachment);
        var actualFile3 = await _fileStorage.PutFileAsync(task, CreateFormFile("image.jpg"), StoredFileType.Image);
        
        var response = await PostRequestAsync(
            Url,
            _jwtToken,
            new GetListRequest()
            {
                EntityId = task.Id,
                EntityType = StorageEntityType.Task
            }
        );
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(3, responseData.Items.Count);
        Assert.Contains(responseData.Items, items => items.Type == StoredFileType.Image);
        Assert.Contains(responseData.Items, items => items.Type == StoredFileType.Attachment);
    }
    
    [Fact]
    public async Task ShouldNotIfHasNotAccessToTask()
    {
        var (jwtToken2, user2,  workspace2) = UserSeeder.CreateAuthorizedAsync().Result;
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var actualFile1 = await _fileStorage.PutFileAsync(task, CreateFormFile(), StoredFileType.Attachment);
        
        var response = await PostRequestAsync(
            Url,
            jwtToken2,
            new GetListRequest()
            {
                EntityId = task.Id,
                EntityType = StorageEntityType.Task
            }
        );
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
