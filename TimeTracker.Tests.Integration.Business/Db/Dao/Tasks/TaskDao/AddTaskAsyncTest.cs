using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tasks.TaskDao;

public class AddTaskAsyncTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectSeeder _projectSeeder;
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly ITaskDao _taskDao;
    private readonly ProjectEntity _project1;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly TaskListEntity _taskList1;
    private readonly ProjectEntity _project2;
    private readonly TaskListEntity _taskList2;

    public AddTaskAsyncTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _taskDao = Scope.Resolve<ITaskDao>();
        _taskFactory = Scope.Resolve<IDataFactory<TaskEntity>>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        
        _project1 = _projectSeeder.CreateAsync(_user.CreatedWorkspaces.First()).Result;
        _taskList1 = _taskListSeeder.CreateAsync(_project1).Result;
        
        _project2 = _projectSeeder.CreateAsync(_user.CreatedWorkspaces.First()).Result;
        _taskList2 = _taskListSeeder.CreateAsync(_project2).Result;
    }

    [Fact]
    public async Task ShouldGenerateTaskIdWithinProject()
    {
        var fakeTask = _taskFactory.Generate();
        
        var firstTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            fakeTask.Title
        );
        Assert.Equal(1, firstTask.TaskId);
        await FlushDbChanges();
        
        fakeTask = _taskFactory.Generate();
        var secondTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            fakeTask.Title
        );
        Assert.Equal(2, secondTask.TaskId);
        await FlushDbChanges();
        
        fakeTask = _taskFactory.Generate();
        var thirdTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            fakeTask.Title
        );
        Assert.Equal(3, thirdTask.TaskId);
    }
    
    [Fact]
    public async Task ShouldGenerateUniqueTaskIdWithinWorkspace()
    {
        var fakeTask = _taskFactory.Generate();
        
        var firstTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            fakeTask.Title
        );
        Assert.Equal(1, firstTask.TaskId);
        await FlushDbChanges();
        
        fakeTask = _taskFactory.Generate();
        var secondTask = await _taskDao.AddTaskAsync(
            _taskList2,
            _user,
            fakeTask.Title
        );
        Assert.Equal(2, secondTask.TaskId);
    }

    [Fact]
    public async Task ShouldAssignTopPositionIndexWithinTaskList()
    {
        var firstTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            _taskFactory.Generate().Title
        );
        await FlushDbChanges();

        var secondTask = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            _taskFactory.Generate().Title
        );
        await FlushDbChanges();

        var thirdTask = await _taskDao.AddTaskAsync(
            _taskList2,
            _user,
            _taskFactory.Generate().Title
        );

        Assert.Equal(0, firstTask.PositionIndex);
        Assert.Equal(-1, secondTask.PositionIndex);
        Assert.Equal(0, thirdTask.PositionIndex);
    }

    [Fact]
    public async Task ShouldUseManualExternalSourceTypeByDefault()
    {
        var task = await _taskDao.AddTaskAsync(
            _taskList1,
            _user,
            _taskFactory.Generate().Title
        );

        Assert.Equal(ExternalSourceType.Manual, task.ExternalSourceType);
    }
}
 
