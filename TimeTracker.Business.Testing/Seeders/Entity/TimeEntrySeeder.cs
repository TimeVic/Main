using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class TimeEntrySeeder: ITimeEntrySeeder
{
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectDao _projectDao;

    public TimeEntrySeeder(
        IDataFactory<TimeEntryEntity> timeEntryFactory,
        IUserSeeder userSeeder,
        ITimeEntryDao timeEntryDao,
        IProjectDao projectDao
    )
    {
        _timeEntryFactory = timeEntryFactory;
        _userSeeder = userSeeder;
        _timeEntryDao = timeEntryDao;
        _projectDao = projectDao;
    }

    public async Task<ICollection<TimeEntryEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var workspace = user.Workspaces.First();
        var project = await _projectDao.CreateAsync(workspace, "Test project name");
        var result = new List<TimeEntryEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _timeEntryFactory.Generate();
            var entry = await _timeEntryDao.SetAsync(
                workspace,
                new TimeEntryCreationDto()
                {
                    Description = fakeEntry.Description,
                    EndTime = fakeEntry.EndTime.Value,
                    StartTime = fakeEntry.StartTime,
                    Date = fakeEntry.Date,
                    HourlyRate = fakeEntry.HourlyRate,
                    IsBillable = fakeEntry.IsBillable
                },
                project
            );
            result.Add(entry);
        }

        return result;
    }

    public async Task<ICollection<TimeEntryEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        return await CreateSeveralAsync(user);
    }
}
