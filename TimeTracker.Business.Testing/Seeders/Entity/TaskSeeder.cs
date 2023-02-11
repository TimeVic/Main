using TimeTracker.Business.Orm.Dao.Task;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class TaskSeeder: ITaskSeeder
{
    private readonly IDataFactory<TaskEntity> _factory;
    private readonly ITaskDao _taskDao;
    private readonly IUserSeeder _userSeeder;

    public TaskSeeder(
        IDataFactory<TaskEntity> factory,
        ITaskDao taskDao,
        IUserSeeder userSeeder
    )
    {
        _factory = factory;
        _taskDao = taskDao;
        _userSeeder = userSeeder;
    }
    
    public async Task<ICollection<TaskEntity>> CreateSeveralAsync(
        TaskListEntity taskList,
        int count = 1,
        UserEntity? user = null
    )
    {
        var result = new List<TaskEntity>();
        for (int i = 0; i < count; i++)
        {
            result.Add(
                await CreateAsync(taskList, user)
            );
        }

        return result;
    }
    
    public async Task<TaskEntity> CreateAsync(TaskListEntity taskList, UserEntity? user = null)
    {
        user ??= await _userSeeder.CreateActivatedAsync();
        
        var fakeEntry = _factory.Generate();
        var entry = await _taskDao.AddTaskAsync(
            taskList,
            user,
            fakeEntry.Title,
            fakeEntry.Description,
            fakeEntry.NotificationTime
        );
        return entry;
    }
}
