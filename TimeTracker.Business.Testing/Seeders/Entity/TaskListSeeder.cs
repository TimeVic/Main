using TimeTracker.Business.Orm.Dao.Task;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class TaskListSeeder: ITaskListSeeder
{
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListDao _taskListDao;
    private readonly IProjectSeeder _projectSeeder;

    public TaskListSeeder(
        IDataFactory<TaskListEntity> taskListFactory,
        ITaskListDao taskListDao,
        IProjectSeeder projectSeeder
    )
    {
        _taskListFactory = taskListFactory;
        _taskListDao = taskListDao;
        _projectSeeder = projectSeeder;
    }
    
    public async Task<ICollection<TaskListEntity>> CreateSeveralAsync(ProjectEntity project, int count = 1)
    {
        var result = new List<TaskListEntity>();
        for (int i = 0; i < count; i++)
        {
            result.Add(
                await CreateAsync(project)
            );
        }

        return result;
    }
    
    public async Task<TaskListEntity> CreateAsync(ProjectEntity project)
    {
        var fakeEntry = _taskListFactory.Generate();
        var entry = await _taskListDao.CreateTaskListAsync(project, fakeEntry.Name);
        return entry;
    }
}
