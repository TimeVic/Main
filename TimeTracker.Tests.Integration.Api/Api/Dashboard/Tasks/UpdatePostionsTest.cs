using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public class UpdatePositionsTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/update-positions";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    
    private readonly TaskListEntity _taskList;
    private readonly TaskEntity _task;
    private readonly TaskListEntity _otherTaskList;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITagSeeder _tagSeeder;

    public UpdatePositionsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        _otherTaskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(_taskList).Result;
    }
    
    [Fact]
    public async Task ShouldUpdatePositions()
    {
        var task2 = _taskSeeder.CreateAsync(_taskList).Result;
        var task3 = _taskSeeder.CreateAsync(_taskList).Result;
        var task4 = _taskSeeder.CreateAsync(_taskList).Result;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdatePositionsRequest()
        {
            TaskListId = _taskList.Id,
            Items = new Dictionary<Guid, int>()
            {
                { _task.Id, 1 },
                { task2.Id, 2 },
                { task3.Id, 3 },
                { task4.Id, 4 },
            }
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await DbSessionProvider.CurrentSession.RefreshAsync(task2);
        await DbSessionProvider.CurrentSession.RefreshAsync(task3);
        await DbSessionProvider.CurrentSession.RefreshAsync(task4);
        
        Assert.Equal(1, _task.PositionIndex);
        Assert.Equal(2, task2.PositionIndex);
        Assert.Equal(3, task3.PositionIndex);
        Assert.Equal(4, task4.PositionIndex);
    }
    
    [Fact]
    public async Task ShouldNotUpdateForOtherTasks()
    {
        var task2 = _taskSeeder.CreateAsync(_taskList).Result;
     
        var task3 = _taskSeeder.CreateAsync(_taskList).Result;
        task3.PositionIndex = 99;
        var task4 = _taskSeeder.CreateAsync(_taskList).Result;
        task4.PositionIndex = 100;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdatePositionsRequest()
        {
            TaskListId = _otherTaskList.Id,
            Items = new Dictionary<Guid, int>()
            {
                { _task.Id, 1 },
                { task2.Id, 2 },
            }
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await DbSessionProvider.CurrentSession.RefreshAsync(task2);
        await DbSessionProvider.CurrentSession.RefreshAsync(task3);
        await DbSessionProvider.CurrentSession.RefreshAsync(task4);
        
        Assert.Equal(1, _task.PositionIndex);
        Assert.Equal(2, task2.PositionIndex);
        Assert.Equal(99, task3.PositionIndex);
        Assert.Equal(100, task4.PositionIndex);
    }
}
