using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity.Task;

public class TaskCommentSeeder: ITaskCommentSeeder
{
    private readonly IDataFactory<TaskCommentEntity> _factory;
    private readonly ITaskDao _taskDao;
    private readonly IUserSeeder _userSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserDao _userDao;
    private readonly ITaskCommentDao _taskCommentDao;
    private readonly ITaskSeeder _taskSeeder;

    public TaskCommentSeeder(
        IDataFactory<TaskCommentEntity> factory,
        ITaskDao taskDao,
        IUserSeeder userSeeder,
        ITaskListSeeder taskListSeeder,
        IProjectSeeder projectSeeder,
        IUserDao userDao,
        ITaskCommentDao taskCommentDao,
        ITaskSeeder taskSeeder
    )
    {
        _factory = factory;
        _taskDao = taskDao;
        _userSeeder = userSeeder;
        _taskListSeeder = taskListSeeder;
        _projectSeeder = projectSeeder;
        _userDao = userDao;
        _taskCommentDao = taskCommentDao;
        _taskSeeder = taskSeeder;
    }
    
    public async Task<ICollection<TaskCommentEntity>> CreateSeveralAsync(
        TaskEntity taskEntity,
        int count = 1,
        UserEntity? user = null
    )
    {
        var result = new List<TaskCommentEntity>();
        for (int i = 0; i < count; i++)
        {
            result.Add(
                await CreateAsync(taskEntity, user)
            );
        }

        return result;
    }
    
    public async Task<TaskCommentEntity> CreateAsync(TaskEntity? task = null, UserEntity? user = null)
    {
        user ??= await _userSeeder.CreateActivatedAsync();
        if (task == null)
        {
            var workspace = (await _userDao.GetUsersWorkspaces(user, MembershipAccessType.Owner)).First();
            var project = await _projectSeeder.CreateAsync(workspace);
            var taskList = await _taskListSeeder.CreateAsync(project);
            task ??= await _taskSeeder.CreateAsync(taskList);
        }
        
        var fakeEntry = _factory.Generate();
        var entry = await _taskCommentDao.AddAsync(
            task,
            user,
            fakeEntry.Comment
        );
        return entry;
    }
}
