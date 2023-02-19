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

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/storage/delete";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly string _jwtToken;
    private readonly IProjectDao _projectDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    
    private readonly TaskEntity _task;
    private readonly IFileStorage _fileStorage;
    private readonly StoredFileEntity _uploadedFile;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        
        (_jwtToken, _user, var workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        
        _uploadedFile = _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(
            string.Format(Url, _uploadedFile.Id),
            new DeleteRequest()
            {
                Id = _uploadedFile.Id
            }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(
            string.Format(Url, _uploadedFile.Id),
            _jwtToken,
            new DeleteRequest()
            {
                Id = _uploadedFile.Id
            }
        );
        response.EnsureSuccessStatusCode();

        var fileContent = await DbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(_uploadedFile.Id);
        Assert.Null(fileContent);
    }
}
