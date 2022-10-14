using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry;

public class GetProjectPaymentsReportTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryReportsDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IPaymentDao _paymentDao;

    public GetProjectPaymentsReportTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
        _reportsDao = Scope.Resolve<ITimeEntryReportsDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.DefaultWorkspace;
    }

    [Fact]
    public async Task ShouldReceiveSimpleReport()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, _user, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow,
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
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(1),
                EndTime = TimeSpan.FromHours(5),
                IsBillable = true,
                HourlyRate = 15
            });
        }
        
        var result = await _reportsDao.GetProjectPaymentsReport(_workspace.Id, _user.Id);

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(180, Math.Round(actualForProject1.Amount));
        Assert.Equal(0, actualForProject1.PaidAmountByClient);
        Assert.Equal(0, actualForProject1.PaidAmountByProject);
        Assert.Equal(TimeSpan.FromHours(15), actualForProject1.TotalDuration);
        Assert.Equal(project1.Client.Id, actualForProject1.ClientId);
        Assert.Equal(project1.Client.Name, actualForProject1.ClientName);
        
        var actualForProject2 = result.FirstOrDefault(item => item.ProjectId == project2.Id);
        Assert.NotNull(actualForProject2);
        Assert.Equal(project2.Id, actualForProject2.ProjectId);
        Assert.Equal(project2.Name, actualForProject2.ProjectName);
        Assert.Equal(120, Math.Round(actualForProject2.Amount));
        Assert.Equal(0, actualForProject2.PaidAmountByClient);
        Assert.Equal(0, actualForProject2.PaidAmountByProject);
        Assert.Equal(TimeSpan.FromHours(12), actualForProject2.TotalDuration);
        Assert.Equal(project2.Client.Id, actualForProject2.ClientId);
        Assert.Equal(project2.Client.Name, actualForProject2.ClientName);
        
        var actualWithoutProject = result.FirstOrDefault(item => item.ProjectId == null);
        Assert.NotNull(actualWithoutProject);
        Assert.Null(actualWithoutProject.ProjectId);
        Assert.Null(actualWithoutProject.ProjectName);
        Assert.Equal(240, Math.Round(actualWithoutProject.Amount));
        Assert.Equal(0, actualWithoutProject.PaidAmountByClient);
        Assert.Equal(0, actualWithoutProject.PaidAmountByProject);
        Assert.Equal(TimeSpan.FromHours(16), actualWithoutProject.TotalDuration);
        Assert.Null(actualWithoutProject.ClientId);
        Assert.Null(actualWithoutProject.ClientName);
    }
    
    [Fact]
    public async Task ShouldReceiveSimpleReportWithCalculatedPayments()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, _user, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 10
            }, project1);
        }

        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1.Client,
            25,
            DateTime.UtcNow,
            project1.Id,
            ""
        );
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1.Client,
            20,
            DateTime.UtcNow,
            project1.Id,
            ""
        );
        await _paymentDao.CreateAsync(
            _workspace,
            _user,
            project1.Client,
            32,
            DateTime.UtcNow
        );
        
        var result = await _reportsDao.GetProjectPaymentsReport(_workspace.Id, _user.Id);

        var actualForProject1 = result.FirstOrDefault(item => item.ProjectId == project1.Id);
        Assert.NotNull(actualForProject1);
        Assert.Equal(project1.Id, actualForProject1.ProjectId);
        Assert.Equal(project1.Name, actualForProject1.ProjectName);
        Assert.Equal(150, Math.Round(actualForProject1.Amount));
        Assert.Equal(77, actualForProject1.PaidAmountByClient);
        Assert.Equal(45, actualForProject1.PaidAmountByProject);
        Assert.Equal(TimeSpan.FromHours(15), actualForProject1.TotalDuration);
        Assert.Equal(project1.Client.Id, actualForProject1.ClientId);
        Assert.Equal(project1.Client.Name, actualForProject1.ClientName);
    }
}
