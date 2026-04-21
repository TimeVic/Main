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

public class SummaryReportIgnoresActiveTimeEntriesTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISummaryReportDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;

    public SummaryReportIgnoresActiveTimeEntriesTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _reportsDao = Scope.Resolve<ISummaryReportDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldIgnoreActiveTimeEntriesForOwnerOrManagerReports()
    {
        var baseDay = DateTime.UtcNow.Date;
        var project = await _projectSeeder.CreateAsync(_workspace);

        await CreateActiveTimeEntryAsync(_user, project, baseDay.AddHours(10));
        await FlushDbChanges();

        Assert.Empty(await _reportsDao.GetReportByProjectForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddDays(-1),
            baseDay.AddDays(1)
        ));
        Assert.Empty(await _reportsDao.GetReportByClientForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddDays(-1),
            baseDay.AddDays(1)
        ));
        Assert.Empty(await _reportsDao.GetReportByUserForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddDays(-1),
            baseDay.AddDays(1)
        ));
        Assert.Empty(await _reportsDao.GetReportByDayForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddDays(-1),
            baseDay.AddDays(1)
        ));
        Assert.Empty(await _reportsDao.GetReportByWeekForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddDays(-7),
            baseDay.AddDays(7)
        ));
        Assert.Empty(await _reportsDao.GetReportByMonthForOwnerOrManagerAsync(
            _workspace.Id,
            baseDay.AddMonths(-1),
            baseDay.AddMonths(1)
        ));
    }

    [Fact]
    public async Task ShouldIgnoreActiveTimeEntriesForOtherReports()
    {
        var baseDay = DateTime.UtcNow.Date;
        var project = await _projectSeeder.CreateAsync(_workspace);
        var otherUser = await _userSeeder.CreateActivatedAsync();

        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = project }
            }
        );

        await CreateActiveTimeEntryAsync(otherUser, project, baseDay.AddHours(10));
        await FlushDbChanges();

        var availableProjects = new List<ProjectEntity> { project };

        Assert.Empty(await _reportsDao.GetReportByProjectForOtherAsync(
            baseDay.AddDays(-1),
            baseDay.AddDays(1),
            otherUser.Id,
            availableProjects
        ));
        Assert.Empty(await _reportsDao.GetReportByClientForOtherAsync(
            baseDay.AddDays(-1),
            baseDay.AddDays(1),
            otherUser.Id,
            availableProjects
        ));
        Assert.Empty(await _reportsDao.GetReportByUserForOtherAsync(
            baseDay.AddDays(-1),
            baseDay.AddDays(1),
            otherUser.Id,
            availableProjects
        ));
        Assert.Empty(await _reportsDao.GetReportByDayForOtherAsync(
            baseDay.AddDays(-1),
            baseDay.AddDays(1),
            otherUser.Id,
            availableProjects
        ));
        Assert.Empty(await _reportsDao.GetReportByWeekForOtherAsync(
            baseDay.AddDays(-7),
            baseDay.AddDays(7),
            otherUser.Id,
            availableProjects
        ));
        Assert.Empty(await _reportsDao.GetReportByMonthForOtherAsync(
            baseDay.AddMonths(-1),
            baseDay.AddMonths(1),
            otherUser.Id,
            availableProjects
        ));
    }

    private async Task CreateActiveTimeEntryAsync(UserEntity user, ProjectEntity project, DateTime startTime)
    {
        await _timeEntryDao.SetAsync(
            user,
            _workspace,
            new TimeEntryCreationDto
            {
                StartTime = startTime,
                EndTime = null,
                IsBillable = true,
                HourlyRate = 10
            },
            project
        );
    }
}
