using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class TaskSeeder: ITaskSeeder
{
    private readonly IDataFactory<TaskEntity> _factory;
    private readonly ITaskDao _taskDao;
    private readonly IUserSeeder _userSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserDao _userDao;

    public TaskSeeder(
        IDataFactory<TaskEntity> factory,
        ITaskDao taskDao,
        IUserSeeder userSeeder,
        ITaskListSeeder taskListSeeder,
        IProjectSeeder projectSeeder,
        IWorkspaceSeeder workspaceSeeder,
        IUserDao userDao
    )
    {
        _factory = factory;
        _taskDao = taskDao;
        _userSeeder = userSeeder;
        _taskListSeeder = taskListSeeder;
        _projectSeeder = projectSeeder;
        _workspaceSeeder = workspaceSeeder;
        _userDao = userDao;
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
    
    public async Task<TaskEntity> CreateAsync(TaskListEntity? taskList = null, UserEntity? user = null)
    {
        user ??= await _userSeeder.CreateActivatedAsync();
        if (taskList == null)
        {
            var workspace = (await _userDao.GetUsersWorkspaces(user, MembershipAccessType.Owner)).First();
            var project = await _projectSeeder.CreateAsync(workspace);
            taskList ??= await _taskListSeeder.CreateAsync(project);    
        }
        
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
