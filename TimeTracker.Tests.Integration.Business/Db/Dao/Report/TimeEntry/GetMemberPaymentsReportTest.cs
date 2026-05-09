using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry;

public class GetMemberPaymentsReportTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryReportsDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IUserDao _userDao;

    public GetMemberPaymentsReportTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IMemberPaymentDao>();
        _reportsDao = Scope.Resolve<ITimeEntryReportsDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveSimpleReport()
    {
        var baseDay = DateTime.UtcNow.Date;
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await FlushDbChanges();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = baseDay.AddHours(10),
                EndTime = baseDay.AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = projects.Last();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = baseDay.AddHours(1),
                EndTime = baseDay.AddHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = baseDay.AddHours(1),
                EndTime = baseDay.AddHours(5),
                IsBillable = true,
                HourlyRate = 15
            });
        }
        
        await FlushDbChanges();
        var result = await _reportsDao.GetProjectMemberPaymentsReport(
            _workspace.Id,
            _user.Id,
            DateTime.UtcNow
        );

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(180, Math.Round(actualForProject1.Amount));
        Assert.Equal(0, actualForProject1.PaidAmountByClient);
        Assert.Equal(0, actualForProject1.PaidAmountByProject);
        AssertDurationHours(15, actualForProject1.TotalDuration);
        Assert.NotNull(project1.Client);
        Assert.Equal(project1.Client!.Id, actualForProject1.ClientId);
        Assert.Equal(project1.Client.Name, actualForProject1.ClientName);
        
        var actualForProject2 = result.FirstOrDefault(item => item.ProjectId == project2.Id);
        Assert.NotNull(actualForProject2);
        Assert.Equal(project2.Id, actualForProject2.ProjectId);
        Assert.Equal(project2.Name, actualForProject2.ProjectName);
        Assert.Equal(120, Math.Round(actualForProject2.Amount));
        Assert.Equal(0, actualForProject2.PaidAmountByClient);
        Assert.Equal(0, actualForProject2.PaidAmountByProject);
        AssertDurationHours(12, actualForProject2.TotalDuration);
        Assert.NotNull(project2.Client);
        Assert.Equal(project2.Client.Id, actualForProject2.ClientId);
        Assert.Equal(project2.Client.Name, actualForProject2.ClientName);
        
        var actualWithoutProject = result.FirstOrDefault(item => item.ProjectId == null);
        Assert.NotNull(actualWithoutProject);
        Assert.Null(actualWithoutProject.ProjectId);
        Assert.Null(actualWithoutProject.ProjectName);
        Assert.Equal(240, Math.Round(actualWithoutProject.Amount));
        Assert.Equal(0, actualWithoutProject.PaidAmountByClient);
        Assert.Equal(0, actualWithoutProject.PaidAmountByProject);
        AssertDurationHours(16, actualWithoutProject.TotalDuration);
        Assert.Null(actualWithoutProject.ClientId);
        Assert.Null(actualWithoutProject.ClientName);
    }
    
    [Fact]
    public async Task ShouldReceiveSimpleReportWithCalculatedMemberPayments()
    {
        var baseDay = DateTime.UtcNow.Date;
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await FlushDbChanges();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = baseDay.AddHours(10),
                EndTime = baseDay.AddHours(15),
                IsBillable = true,
                HourlyRate = 10
            }, project1);
        }

        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            25,
            DateTime.UtcNow,
            ""
        );
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            20,
            DateTime.UtcNow,
            ""
        );
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            32,
            DateTime.UtcNow
        );
        
        await FlushDbChanges();
        var result = await _reportsDao.GetProjectMemberPaymentsReport(
            _workspace.Id,
            _user.Id,
            DateTime.UtcNow
        );

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(150, Math.Round(actualForProject1.Amount));
        Assert.Equal(77, actualForProject1.PaidAmountByClient);
        Assert.Equal(77, actualForProject1.PaidAmountByProject);
        AssertDurationHours(15, actualForProject1.TotalDuration);
        var projectClient = project1.Client!;
        Assert.Equal(projectClient.Id, actualForProject1.ClientId);
        Assert.Equal(projectClient.Name, actualForProject1.ClientName);
    }

    [Fact]
    public async Task ShouldIncludeCurrentDayMemberPaymentWithoutTimeEntries()
    {
        var project = (await _projectSeederSeeder.CreateSeveralAsync(_workspace, 1)).First();
        var client = project.Client!;
        var paymentTime = DateTime.UtcNow.Date.AddHours(12);
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project,
            125,
            paymentTime
        );

        await FlushDbChanges();
        var result = await _reportsDao.GetProjectMemberPaymentsReport(
            _workspace.Id,
            _user.Id,
            DateTime.UtcNow.Date
        );

        var actualForClient = result.FirstOrDefault(item => item.ClientId == client.Id);
        Assert.NotNull(actualForClient);
        Assert.Equal(project.Id, actualForClient.ProjectId);
        Assert.Equal(client.Id, actualForClient.ClientId);
        Assert.Equal(client.Name, actualForClient.ClientName);
        Assert.Equal(0, actualForClient.Amount);
        Assert.Equal(125, actualForClient.PaidAmountByClient);
        Assert.Equal(125, actualForClient.PaidAmountByProject);
        Assert.Equal(TimeSpan.Zero, actualForClient.TotalDuration);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyForCurrentUser()
    {
        var baseDay = DateTime.UtcNow.Date;
        var otherUser = await _userSeeder.CreateActivatedAndShareAsync(_workspace);
        await FlushDbChanges();
        
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        var project1 = projects.First();
       
        await _paymentDao.CreateAsync(
            _workspace,
            otherUser,
            project1,
            15,
            DateTime.UtcNow,
            ""
        );
        await _timeEntryDao.SetAsync(otherUser, _workspace, new TimeEntryCreationDto()
        {
            StartTime = baseDay.AddHours(10),
            EndTime = baseDay.AddHours(15),
            IsBillable = true,
            HourlyRate = 2
        }, project1);
        
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            StartTime = baseDay.AddHours(10),
            EndTime = baseDay.AddHours(15),
            IsBillable = true,
            HourlyRate = 1
        }, project1);
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            10,
            DateTime.UtcNow,
            ""
        );

        await FlushDbChanges();
        var result = await _reportsDao.GetProjectMemberPaymentsReport(
            _workspace.Id,
            _user.Id,
            DateTime.UtcNow
        );

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(5, Math.Round(actualForProject1.Amount));
        Assert.Equal(10, actualForProject1.PaidAmountByClient);
        Assert.Equal(10, actualForProject1.PaidAmountByProject);
        AssertDurationHours(5, actualForProject1.TotalDuration);
        var projectClient = project1.Client!;
        Assert.Equal(projectClient.Id, actualForProject1.ClientId);
        Assert.Equal(projectClient.Name, actualForProject1.ClientName);
    }
    
    [Fact]
    public async Task ShouldReceiveForProvidedDate()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        var project1 = projects.First();
       
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            15,
            DateTime.UtcNow.AddDays(-4),
            ""
        );
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1,
            15,
            DateTime.UtcNow.AddDays(-10),
            ""
        );
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            StartTime = DateTime.UtcNow.AddDays(-4).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(-4).AddHours(15),
        }, project1);
        
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            StartTime = DateTime.UtcNow.AddDays(-10).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(-10).AddHours(15),
            IsBillable = true,
            HourlyRate = 1
        }, project1);
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            StartTime = DateTime.UtcNow.AddDays(-4).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(-4).AddHours(15),
            IsBillable = true,
            HourlyRate = 1
        }, project1);
        await FlushDbChanges();

        await FlushDbChanges();
        var result = await _reportsDao.GetProjectMemberPaymentsReport(
            _workspace.Id,
            _user.Id,
            DateTime.UtcNow.AddDays(-5)
        );

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(5, Math.Round(actualForProject1.Amount));
        Assert.Equal(15, actualForProject1.PaidAmountByClient);
        Assert.Equal(15, actualForProject1.PaidAmountByProject);
        AssertDurationHours(5, actualForProject1.TotalDuration);
    }

    private static void AssertDurationHours(double expectedHours, TimeSpan actualDuration)
    {
        Assert.Equal(
            (int)TimeSpan.FromHours(expectedHours).TotalSeconds,
            (int)Math.Round(actualDuration.TotalSeconds)
        );
    }
}
