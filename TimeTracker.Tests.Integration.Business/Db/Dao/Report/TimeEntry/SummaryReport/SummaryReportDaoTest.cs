using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry.SummaryReport;

public class SummaryReportDaoTest : BaseTest
{
    private readonly IProjectSeeder _projectSeeder;
    private readonly ISummaryReportDao _reportDao;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly IUserSeeder _userSeeder;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;

    public SummaryReportDaoTest() : base()
    {
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _reportDao = Scope.Resolve<ISummaryReportDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userDao = Scope.Resolve<IUserDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _workspaceSeeder = Scope.Resolve<IWorkspaceSeeder>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ReturnsOnlyCurrentUsersEntriesInCurrentWorkspace()
    {
        var date = DateTime.UtcNow.Date.AddDays(-1);
        var project = await _projectSeeder.CreateAsync(_workspace);
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var otherWorkspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        var otherWorkspaceProject = await _projectSeeder.CreateAsync(otherWorkspace);

        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel> { new() { Project = project } }
        );
        await _timeEntryDao.SetAsync(
            _user,
            _workspace,
            CreateTimeEntry(date.AddHours(10), date.AddHours(15)),
            project
        );
        await _timeEntryDao.SetAsync(
            otherUser,
            _workspace,
            CreateTimeEntry(date.AddHours(10), date.AddHours(14)),
            project
        );
        await _timeEntryDao.SetAsync(
            _user,
            otherWorkspace,
            CreateTimeEntry(date.AddHours(10), date.AddHours(15)),
            otherWorkspaceProject
        );
        await FlushDbChanges();

        var byDay = await _reportDao.GetReportByDayAsync(_workspace.Id, _user.Id, date, date);
        var byClient = await _reportDao.GetReportByClientAsync(_workspace.Id, _user.Id, date, date);
        var byProject = await _reportDao.GetReportByProjectAsync(_workspace.Id, _user.Id, date, date);
        var byMonth = await _reportDao.GetReportByMonthAsync(_workspace.Id, _user.Id, date, date);
        var byWeek = await _reportDao.GetReportByWeekAsync(_workspace.Id, _user.Id, date, date);

        AssertSummary(byDay.Single().Duration, byDay.Single().Amount);
        AssertSummary(byClient.Single().Duration, byClient.Single().Amount);
        AssertSummary(byProject.Single().Duration, byProject.Single().Amount);
        AssertSummary(byMonth.Single().Duration, byMonth.Single().Amount);
        AssertSummary(byWeek.Single().Duration, byWeek.Single().Amount);
    }

    [Fact]
    public async Task DoesNotIncludeActiveTimeEntries()
    {
        var date = DateTime.UtcNow.Date;
        await _timeEntryDao.SetAsync(
            _user,
            _workspace,
            new TimeEntryCreationDto { StartTime = date.AddHours(10), IsBillable = true, HourlyRate = 12 }
        );
        await FlushDbChanges();

        Assert.Empty(await _reportDao.GetReportByDayAsync(_workspace.Id, _user.Id, date, date));
        Assert.Empty(await _reportDao.GetReportByClientAsync(_workspace.Id, _user.Id, date, date));
        Assert.Empty(await _reportDao.GetReportByProjectAsync(_workspace.Id, _user.Id, date, date));
        Assert.Empty(await _reportDao.GetReportByMonthAsync(_workspace.Id, _user.Id, date, date));
        Assert.Empty(await _reportDao.GetReportByWeekAsync(_workspace.Id, _user.Id, date, date));
    }

    private static TimeEntryCreationDto CreateTimeEntry(DateTime startTime, DateTime endTime)
    {
        return new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = endTime,
            IsBillable = true,
            HourlyRate = 12
        };
    }

    private static void AssertSummary(TimeSpan duration, decimal amount)
    {
        Assert.Equal(TimeSpan.FromHours(5), duration);
        Assert.Equal(60m, amount);
    }
}
