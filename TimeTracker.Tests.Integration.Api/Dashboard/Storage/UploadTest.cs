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
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Storage;

public class UploadTest: BaseTest
{
    private readonly string Url = "/dashboard/storage/upload";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly string _jwtToken;
    private readonly IProjectDao _projectDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    
    private readonly TaskEntity _task;

    public UploadTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        
        (_jwtToken, _user, var workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            data: new Dictionary<string, object>()
            {
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            file: CreateFormFile()
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpload()
    {
        Assert.Equal(0, _task.Attachments.Count);

        var fileToUpload = CreateFormFile("test.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        Assert.True(actualData.Id > 0);
        Assert.NotEmpty(actualData.Url);
        Assert.NotEmpty(actualData.ThumbUrl);

        await CommitDbChanges();
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        Assert.Equal(1, actualTask.Attachments.Count);
    }
    
    [Fact]
    public async Task ShouldUploadIfHasNotAccessToEntity()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var task = _taskSeeder.CreateAsync(user: user2).Result;
        
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "EntityId", task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            CreateFormFile("test.jpg")
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldUploadBigZipFile()
    {
        Assert.Equal(0, _task.Attachments.Count);

        var fileToUpload = CreateFormFile("big.jpg");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>()
            {
                { "EntityId", _task.Id },
                { "EntityType", StorageEntityType.Task },
                { "FileType", StoredFileType.Attachment },
            },
            fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<StoredFileDto>();
        Assert.True(actualData.Id > 0);
        Assert.NotEmpty(actualData.Url);
        Assert.NotEmpty(actualData.ThumbUrl);

        await CommitDbChanges();
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        Assert.Equal(1, actualTask.Attachments.Count);
    }
}
