using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity.Task;

public class TimeEntrySeeder: ITimeEntrySeeder
{
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectSeeder _projectSeeder;

    public TimeEntrySeeder(
        IDataFactory<TimeEntryEntity> timeEntryFactory,
        IUserSeeder userSeeder,
        ITimeEntryDao timeEntryDao,
        IProjectSeeder projectSeeder
    )
    {
        _timeEntryFactory = timeEntryFactory;
        _userSeeder = userSeeder;
        _timeEntryDao = timeEntryDao;
        _projectSeeder = projectSeeder;
    }

    public async Task<TimeEntryEntity> CreateAsync(WorkspaceEntity workspace, UserEntity user, ProjectEntity? project = null)
    {
        var fakeEntry = _timeEntryFactory.Generate();
        var entry = await _timeEntryDao.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto()
            {
                Description = fakeEntry.Description,
                EndTime = fakeEntry.EndTime!.Value,
                StartTime = fakeEntry.StartTime,
                HourlyRate = fakeEntry.HourlyRate,
                IsBillable = fakeEntry.IsBillable
            },
            project
        );
        return entry;
    }
    
    public async Task<ICollection<TimeEntryEntity>> CreateSeveralAsync(WorkspaceEntity workspace, UserEntity user, int count = 1, ProjectEntity? project = null)
    {
        project ??= await _projectSeeder.CreateAsync(workspace);
        var result = new List<TimeEntryEntity>();
        for (int i = 0; i < count; i++)
        {
            var entry = await CreateAsync(workspace, user, project);
            result.Add(entry);
        }

        return result;
    }
}
