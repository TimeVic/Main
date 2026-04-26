using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tasks.TaskDao;

public class GetTrackedDurationByTaskIdsTest : BaseTest
{
    private readonly ITaskDao _taskDao;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITaskSeeder _taskSeeder;

    public GetTrackedDurationByTaskIdsTest() : base()
    {
        _taskDao = Scope.Resolve<ITaskDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
    }

    [Fact]
    public async Task ShouldReturnTrackedDurationForRequestedTaskIds()
    {
        var now = DateTime.UtcNow;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.CreatedWorkspaces.First();
        var project = await _projectSeeder.CreateAsync(workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var targetTask = await _taskSeeder.CreateAsync(taskList, user);
        var otherTask = await _taskSeeder.CreateAsync(taskList, user);

        var firstEntry = await _timeEntryDao.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto
            {
                Description = "Tracked 1",
                StartTime = now.AddHours(-7),
                EndTime = now.AddHours(-5),
                IsBillable = false
            },
            project
        );
        firstEntry.Task = targetTask;
        await DbSessionProvider.CurrentSession.SaveAsync(firstEntry);

        var secondEntry = await _timeEntryDao.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto
            {
                Description = "Tracked 2",
                StartTime = now.AddHours(-4),
                EndTime = now.AddHours(-3),
                IsBillable = false
            },
            project
        );
        secondEntry.Task = targetTask;
        await DbSessionProvider.CurrentSession.SaveAsync(secondEntry);

        var deletedEntry = await _timeEntryDao.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto
            {
                Description = "Ignored deleted",
                StartTime = now.AddHours(-3),
                EndTime = now.AddHours(-2),
                IsBillable = false
            },
            project
        );
        deletedEntry.Task = targetTask;
        deletedEntry.IsMarkedToDelete = true;
        await DbSessionProvider.CurrentSession.SaveAsync(deletedEntry);

        var activeEntry = await _timeEntryDao.StartNewAsync(
            user,
            workspace,
            now.AddHours(-1),
            projectId: project.Id,
            internalTask: targetTask
        );
        await DbSessionProvider.CurrentSession.SaveAsync(activeEntry);

        var otherTaskEntry = await _timeEntryDao.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto
            {
                Description = "Other task",
                StartTime = now.AddHours(-6),
                EndTime = now.AddHours(-5),
                IsBillable = false
            },
            project
        );
        otherTaskEntry.Task = otherTask;
        await DbSessionProvider.CurrentSession.SaveAsync(otherTaskEntry);

        await FlushDbChanges(true);

        var result = await _taskDao.GetTrackedDurationByTaskIds(new[] { targetTask.Id, otherTask.Id });

        Assert.Equal(TimeSpan.FromHours(3), result[targetTask.Id]);
        Assert.Equal(TimeSpan.FromHours(1), result[otherTask.Id]);
    }
}
