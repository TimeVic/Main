using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry.SummaryReport;

public class GetReportByProjectTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISummaryReportDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IPaymentDao _paymentDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetReportByProjectTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
        _reportsDao = Scope.Resolve<ISummaryReportDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task ShouldReceiveReportForOwnerOrManager()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = projects.Last();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(1),
                EndTime = TimeSpan.FromHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(1),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }
        
        var result = await _reportsDao.GetReportByProjectForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Skip(1).First();
        var thirdReportItem = result.Last();
        Assert.Equal(project1.Id, firstReportItem.ProjectId);
        Assert.Equal(project2.Id, secondReportItem.ProjectId);
        Assert.Null(thirdReportItem.ProjectId);
        
        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(24), thirdReportItem.Duration);
        
        result = await _reportsDao.GetReportByProjectForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        firstReportItem = result.First();
        secondReportItem = result.Skip(1).First();
        thirdReportItem = result.Last();
        Assert.Equal(project1.Id, firstReportItem.ProjectId);
        Assert.Equal(project2.Id, secondReportItem.ProjectId);
        Assert.Null(thirdReportItem.ProjectId);
        
        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(24), thirdReportItem.Duration);
    }
    
    [Fact]
    public async Task ShouldReceiveReportForOther()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = projects.Last();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(1),
                EndTime = TimeSpan.FromHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(1),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        var result = await _reportsDao.GetReportByProjectForOtherAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            new List<ProjectEntity> { project1 }
        );
        Assert.Equal(1, result.Count);
        
        var firstReportItem = result.First();

        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
    }
}
