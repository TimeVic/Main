using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry.SummaryReport;

public class GetReportByWeeksTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISummaryReportDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IPaymentDao _paymentDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;

    public GetReportByWeeksTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
        _reportsDao = Scope.Resolve<ISummaryReportDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
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
                Date = DateTime.UtcNow.AddDays(-7),
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
                Date = DateTime.UtcNow.AddDays(-14),
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
                Date = DateTime.UtcNow.AddDays(-21),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        await CommitDbChanges();
        
        var result = await _reportsDao.GetReportByWeekForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddMonths(-21),
            DateTime.UtcNow
        );
        Assert.Equal(3, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Skip(1).First();
        var thirdReportItem = result.Last();

        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(24), thirdReportItem.Duration);
        
        Assert.Equal(180m, firstReportItem.Amount);
        Assert.Equal(120m, secondReportItem.Amount);
        Assert.Equal(360m, thirdReportItem.Amount);
        
        result = await _reportsDao.GetReportByWeekForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddMonths(-3),
            DateTime.UtcNow
        );
        Assert.Equal(3, result.Count);
        
        firstReportItem = result.First();
        secondReportItem = result.Skip(1).First();
        thirdReportItem = result.Last();

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
        var project2 = projects.Last();
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new() { Project = project1 },
                new() { Project = project2 }
            }
        );
        
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(otherUser, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var user2 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user2, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-14),
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
                Date = DateTime.UtcNow.AddDays(-21),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        var result = await _reportsDao.GetReportByWeekForOtherAsync(
            DateTime.UtcNow.AddDays(-21),
            DateTime.UtcNow,
            otherUser.Id,
            new List<ProjectEntity> { project2, project1 }
        );
        Assert.Equal(2, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Last();

        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), secondReportItem.Duration);
        
        Assert.Equal(180, firstReportItem.Amount);
        Assert.Equal(0, secondReportItem.Amount);
    }
    
    [Fact]
    public async Task ShouldReceiveCorrectWeekNumber()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Date = DateTime.Parse("2021-12-26T01:00:00Z"),
            StartTime = TimeSpan.FromHours(1),
            EndTime = TimeSpan.FromHours(2),
            IsBillable = true,
            HourlyRate = 12
        }, project1);
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Date = DateTime.Parse("2021-12-31T01:00:00Z"),
            StartTime = TimeSpan.FromHours(2),
            EndTime = TimeSpan.FromHours(4),
            IsBillable = true,
            HourlyRate = 12
        }, project1);
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Date = DateTime.Parse("2022-01-01T01:00:00Z"),
            StartTime = TimeSpan.FromHours(2),
            EndTime = TimeSpan.FromHours(4),
            IsBillable = true,
            HourlyRate = 12
        }, project1);
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Date = DateTime.Parse("2022-01-03T01:00:00Z"),
            StartTime = TimeSpan.FromHours(5),
            EndTime = TimeSpan.FromHours(8),
            IsBillable = true,
            HourlyRate = 12
        }, project1);
        await CommitDbChanges();
        
        var result = await _reportsDao.GetReportByWeekForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.Parse("2021-12-15T01:00:00Z"),
            DateTime.Parse("2022-01-15T01:00:00Z")
        );
        Assert.Equal(3, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Skip(1).First();
        var thirdReportItem = result.Last();
        Assert.Equal(DateTime.Parse("2021-12-20T00:00:00Z").ToUniversalTime(), thirdReportItem.WeekStartDate);
        Assert.Equal(DateTime.Parse("2021-12-26T00:00:00Z").ToUniversalTime(), thirdReportItem.WeekEndDate);
        Assert.Equal(DateTime.Parse("2021-12-27T00:00:00Z").ToUniversalTime(), secondReportItem.WeekStartDate);
        Assert.Equal(DateTime.Parse("2022-01-02T00:00:00Z").ToUniversalTime(), secondReportItem.WeekEndDate);
        Assert.Equal(DateTime.Parse("2022-01-03T00:00:00Z").ToUniversalTime(), firstReportItem.WeekStartDate);
        Assert.Equal(DateTime.Parse("2022-01-09T00:00:00Z").ToUniversalTime(), firstReportItem.WeekEndDate);

        Assert.Equal(TimeSpan.FromHours(3), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(4), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(1), thirdReportItem.Duration);
    }
}
