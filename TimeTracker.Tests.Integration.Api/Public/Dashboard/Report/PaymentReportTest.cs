using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Report;

public class PaymentReportTest: BaseTest
{
    private readonly string Url = "/dashboard/report/payments";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryReportsDao _timeEntryReportDao;
    private readonly IPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectDao;

    public PaymentReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _paymentDao = ServiceProvider.GetRequiredService<IPaymentDao>();
        _timeEntryReportDao = ServiceProvider.GetRequiredService<ITimeEntryReportsDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new PaymentReportRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceivePaymentReport()
    {
        var projects = await _projectDao.CreateSeveralAsync(_defaultWorkspace, 2);
        await DbSessionProvider.PerformCommitAsync();
        var project1 = projects.First();
        await _timeEntryDao.SetAsync(_user, _defaultWorkspace, new TimeEntryCreationDto()
        {
            Date = DateTime.UtcNow,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(15),
            IsBillable = true,
            HourlyRate = 10
        }, project1);
        
        await _paymentDao.CreateAsync(
            _defaultWorkspace, 
            _user, 
            project1.Client,
            120,
            DateTime.UtcNow,
            project1.Id,
            ""
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new PaymentReportRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndDate = DateTime.UtcNow
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<PaymentReportResponse>();
        Assert.Equal(1, actualDto.Items.Count);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Amount > 0);
            Assert.True(item.ClientId > 0);
            Assert.True(item.ProjectId > 0);
            Assert.True(item.TotalDuration > TimeSpan.MinValue);
            Assert.True(item.PaidAmountByClient > 0);
            Assert.True(item.PaidAmountByProject > 0);
            Assert.NotEmpty(item.ClientName);
            Assert.NotEmpty(item.ProjectName);
        });
    }
}
