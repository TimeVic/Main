using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
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
    private readonly IProjectSeeder _projectSeeder;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;

    public GetReportByProjectTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IMemberPaymentDao>();
        _reportsDao = Scope.Resolve<ISummaryReportDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveReportForOwnerOrManager()
    {
        var currentDate = DateTime.UtcNow.Date;
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, 2);
        await FlushDbChanges();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = currentDate.AddDays(-1).AddHours(10),
                EndTime = currentDate.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = projects.Last();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = currentDate.AddHours(1),
                EndTime = currentDate.AddHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = currentDate.AddDays(1).AddHours(5),
                EndTime = currentDate.AddDays(1).AddHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }
        
        await FlushDbChanges();
        var result = await _reportsDao.GetReportByProjectForOwnerOrManagerAsync(
            _workspace.Id,
            currentDate.AddDays(-1),
            currentDate.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        var noProjectItem = result.Single(item => item.ProjectId == null);
        var projectItems = result.Where(item => item.ProjectId != null).ToList();
        Assert.Equal(2, projectItems.Count);
        Assert.Contains(projectItems, item => item.ProjectId == project1.Id || item.ProjectId == project2.Id);
        Assert.Contains(projectItems, item => item.ProjectId == project1.Id || item.ProjectId == project2.Id);

        AssertDurationHours(24, noProjectItem.Duration);
        Assert.Equal(360m, noProjectItem.Amount);
        Assert.Contains(projectItems, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(15)) && item.Amount == 180m);
        Assert.Contains(projectItems, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(12)) && item.Amount == 120m);
        
        await FlushDbChanges();
        result = await _reportsDao.GetReportByProjectForOwnerOrManagerAsync(
            _workspace.Id,
            currentDate.AddDays(-1),
            currentDate.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        noProjectItem = result.Single(item => item.ProjectId == null);
        projectItems = result.Where(item => item.ProjectId != null).ToList();
        Assert.Equal(2, projectItems.Count);
        AssertDurationHours(24, noProjectItem.Duration);
        Assert.Contains(projectItems, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(15)));
        Assert.Contains(projectItems, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(12)));
    }
    
    [Fact]
    public async Task ShouldReceiveReportForOther()
    {
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, 2);
        await FlushDbChanges();
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
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(10),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(5),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        await FlushDbChanges();
        var result = await _reportsDao.GetReportByProjectForOtherAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            otherUser.Id,
            new List<ProjectEntity> { project1, project2 }
        );
        Assert.Equal(2, result.Count);
        
        Assert.Contains(result, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(15)) && item.Amount == 180);
        Assert.Contains(result, item => DurationSeconds(item.Duration) == DurationSeconds(TimeSpan.FromHours(12)) && item.Amount == 0);
    }

    private static void AssertDurationHours(double expectedHours, TimeSpan actualDuration)
    {
        Assert.Equal(
            (int)TimeSpan.FromHours(expectedHours).TotalSeconds,
            DurationSeconds(actualDuration)
        );
    }

    private static int DurationSeconds(TimeSpan duration)
    {
        return (int)Math.Round(duration.TotalSeconds);
    }

}
