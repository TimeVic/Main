using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Report;

public class MemberPaymentReportTest: BaseTest
{
    private readonly string Url = "/dashboard/report/member-payments";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryReportsDao _timeEntryReportDao;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectDao;

    public MemberPaymentReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _paymentDao = ServiceProvider.GetRequiredService<IMemberPaymentDao>();
        _timeEntryReportDao = ServiceProvider.GetRequiredService<ITimeEntryReportsDao>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new MemberPaymentReportRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveMemberPaymentReport()
    {
        var projects = await _projectDao.CreateSeveralAsync(_defaultWorkspace, 2);
        await FlushDbChanges();
        var project1 = projects.First();
        await _timeEntryDao.SetAsync(_user, _defaultWorkspace, new TimeEntryCreationDto()
        {
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(15),
            IsBillable = true,
            HourlyRate = 10
        }, project1);
        
        await _paymentDao.CreateAsync(
            _defaultWorkspace, 
            _user, 
            project1,
            120,
            DateTime.UtcNow,
            ""
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new MemberPaymentReportRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndDate = DateTime.UtcNow
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<MemberPaymentReportResponse>();
        Assert.Equal(1, actualDto.Items.Count);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Amount > 0);
            Assert.NotEqual(Guid.Empty, item.ClientId);
            Assert.NotEqual(Guid.Empty, item.ProjectId);
            Assert.True(item.TotalDuration > TimeSpan.MinValue);
            Assert.True(item.PaidAmountByClient > 0);
            Assert.True(item.PaidAmountByProject > 0);
            Assert.NotEmpty(item.ClientName!);
            Assert.NotEmpty(item.ProjectName!);
        });
    }

    [Fact]
    public async Task ShouldIncludeCurrentDayMemberPaymentWithoutTimeEntries()
    {
        var project = (await _projectDao.CreateSeveralAsync(_defaultWorkspace, 1)).First();
        await _paymentDao.CreateAsync(
            _defaultWorkspace,
            _user,
            project,
            125,
            DateTime.UtcNow.Date.AddHours(12)
        );
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new MemberPaymentReportRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndDate = DateTime.UtcNow.Date
        });

        response.EnsureSuccessStatusCode();
        var actualDto = await response.GetJsonDataAsync<MemberPaymentReportResponse>();
        var client = project.Client!;
        var actualForClient = actualDto.Items.FirstOrDefault(item => item.ClientId == client.Id);
        Assert.NotNull(actualForClient);
        Assert.Equal(project.Id, actualForClient.ProjectId);
        Assert.Equal(client.Id, actualForClient.ClientId);
        Assert.Equal(client.Name, actualForClient.ClientName);
        Assert.Equal(0, actualForClient.Amount);
        Assert.Equal(125, actualForClient.PaidAmountByClient);
        Assert.Equal(125, actualForClient.PaidAmountByProject);
        Assert.Equal(TimeSpan.Zero, actualForClient.TotalDuration);
    }
}
