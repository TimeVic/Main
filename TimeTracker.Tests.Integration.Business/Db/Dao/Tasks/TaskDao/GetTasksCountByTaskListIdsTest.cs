using Autofac;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tasks.TaskDao;

public class GetTasksCountByTaskListIdsTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly ITaskDao _taskDao;
    private readonly ITaskListSeeder _taskListSeeder;

    public GetTasksCountByTaskListIdsTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _taskDao = Scope.Resolve<ITaskDao>();
        _taskFactory = Scope.Resolve<IDataFactory<TaskEntity>>();
    }

    [Fact]
    public async Task ShouldReturnActiveTasksCountForRequestedTaskLists()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var project = await _projectSeeder.CreateAsync(user.CreatedWorkspaces.First());
        var firstTaskList = await _taskListSeeder.CreateAsync(project);
        var secondTaskList = await _taskListSeeder.CreateAsync(project);
        var emptyTaskList = await _taskListSeeder.CreateAsync(project);

        await CreateTasksAsync(firstTaskList, user, 3);
        await CreateTasksAsync(secondTaskList, user, 2);
        await _taskDao.AddTaskAsync(secondTaskList, user, _taskFactory.Generate().Title, isArchived: true);
        await FlushDbChanges(true);

        var result = await _taskDao.GetTasksCountByTaskListIds(new[]
        {
            firstTaskList,
            secondTaskList,
            emptyTaskList
        });

        Assert.Equal(3, result[firstTaskList.Id]);
        Assert.Equal(2, result[secondTaskList.Id]);
        Assert.False(result.ContainsKey(emptyTaskList.Id));
    }

    [Fact]
    public async Task ShouldReturnEmptyMapForEmptyTaskListIds()
    {
        var result = await _taskDao.GetTasksCountByTaskListIds(Array.Empty<TaskListEntity>());

        Assert.Empty(result);
    }

    private async Task CreateTasksAsync(TaskListEntity taskList, UserEntity user, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await _taskDao.AddTaskAsync(taskList, user, _taskFactory.Generate().Title);
        }
    }
}
