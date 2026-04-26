using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public class GetOneTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/get-one";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskListEntity _taskList;
    
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IFileStorage _fileStorage;
    private readonly ITagSeeder _tagSeeder;
    private readonly TaskEntity _task;

    public GetOneTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        
        _task = _taskSeeder.CreateAsync(_taskList).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetOneRequest()
        {
            TaskId = _task.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        await _fileStorage.PutFileAsync(_task, CreateFormFile(), StoredFileType.Attachment);
        var now = DateTime.UtcNow;

        _task.OriginalEstimate = TimeSpan.FromHours(5);
        await DbSessionProvider.CurrentSession.SaveAsync(_task);

        var firstEntry = await _timeEntryDao.SetAsync(
            _user,
            _workspace,
            new TimeEntryCreationDto
            {
                Description = "Tracked entry 1",
                StartTime = now.AddHours(-5),
                EndTime = now.AddHours(-3),
                IsBillable = false
            },
            _project
        );
        firstEntry.Task = _task;
        await DbSessionProvider.CurrentSession.SaveAsync(firstEntry);

        var secondEntry = await _timeEntryDao.SetAsync(
            _user,
            _workspace,
            new TimeEntryCreationDto
            {
                Description = "Tracked entry 2",
                StartTime = now.AddHours(-2),
                EndTime = now.AddHours(-1),
                IsBillable = false
            },
            _project
        );
        secondEntry.Task = _task;
        await DbSessionProvider.CurrentSession.SaveAsync(secondEntry);
        await FlushDbChanges(true);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetOneRequest()
        {
            TaskId = _task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.True(actualDto.TaskId > 0);
        Assert.NotEmpty(actualDto.Title);
        Assert.NotEmpty(actualDto.Description!);
        Assert.NotNull(actualDto.TaskList?.Project?.Client);
        Assert.Equal(_taskList.Id, actualDto.TaskList.Id);
        Assert.Equal(TimeSpan.FromHours(5), actualDto.OriginalEstimate);
        Assert.Equal(TimeSpan.FromHours(3), actualDto.TrackedDuration);
        
        Assert.NotEmpty(actualDto.Attachments);
        Assert.NotEmpty(actualDto.Attachments.First().Url);
    }
    
    [Fact]
    public async Task ShouldReceiveIfHasAccess()
    {
        var (_otherToken, _otherUser, _otherWorkspace) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project
                }
            }
        );
        
        var response = await PostRequestAsync(Url, _otherToken, new GetOneRequest()
        {
            TaskId = _task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.True(actualDto.TaskId > 0);
    }
    
    [Fact]
    public async Task ShouldNotReceiveIfHasNoAccessToProject()
    {
        var (_otherToken, _otherUser, _otherWorkspace) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        
        var response = await PostRequestAsync(Url, _otherToken, new GetOneRequest()
        {
            TaskId = _task.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
}
