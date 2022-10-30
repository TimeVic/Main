using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Report.SummaryReportTest;

public class ForOwnerTest: BaseTest
{
    private readonly string Url = "/dashboard/report/summary";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryReportsDao _timeEntryReportDao;
    private readonly IPaymentDao _paymentDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly ClientEntity _client;

    public ForOwnerTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _paymentDao = ServiceProvider.GetRequiredService<IPaymentDao>();
        _timeEntryReportDao = ServiceProvider.GetRequiredService<ITimeEntryReportsDao>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace = _user.Workspaces.First();
        
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client;
        for (int i = 0; i < 3; i++)
        {
            _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-32),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, _project).Wait();
        }
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByDay()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByDay
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();
        
        // Validate Chart items
        Assert.Equal(1, actualDto.ByDays.Count);
        Assert.All(actualDto.ByDays, item =>
        {
            Assert.Equal(DateTime.UtcNow.AddDays(-32).StartOfDay(), item.Date.ToUniversalTime());
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
        
        Assert.NotNull(actualDto.GroupedByDay);
        Assert.Equal(1, actualDto.GroupedByDay.Count);
        Assert.All(actualDto.GroupedByDay, item =>
        {
            Assert.Equal(DateTime.UtcNow.AddDays(-32).StartOfDay(), item.Date.ToUniversalTime());
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByMonth()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByMonth
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();

        Assert.NotNull(actualDto.GroupedByMonth);
        Assert.Equal(1, actualDto.GroupedByMonth.Count);
        Assert.All(actualDto.GroupedByMonth, item =>
        {
            Assert.Equal(DateTime.UtcNow.AddDays(-32).Month, item.Month);
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByWeek()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByWeek
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();

        Assert.NotNull(actualDto.GroupedByWeek);
        Assert.Equal(1, actualDto.GroupedByWeek.Count);
        Assert.All(actualDto.GroupedByWeek, item =>
        {
            Assert.NotNull(item.WeekStartDate);
            Assert.NotNull(item.WeekEndDate);
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByClients()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByClient
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();

        Assert.NotNull(actualDto.GroupedByClient);
        Assert.Equal(1, actualDto.GroupedByClient.Count);
        Assert.All(actualDto.GroupedByClient, item =>
        {
            Assert.Equal(_client.Id, item.ClientId);
            Assert.Equal(_client.Name, item.ClientName);
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByProjects()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByProject
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();

        Assert.NotNull(actualDto.GroupedByProject);
        Assert.Equal(1, actualDto.GroupedByProject.Count);
        Assert.All(actualDto.GroupedByProject, item =>
        {
            Assert.Equal(_project.Id, item.ProjectId);
            Assert.Equal(_project.Name, item.ProjectName);
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveGroupedByUsers()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SummaryReportRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = DateTime.UtcNow.AddDays(-32),
            EndTime = DateTime.UtcNow,
            Type = SummaryReportType.GroupByUser
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<SummaryReportResponse>();

        Assert.NotNull(actualDto.GroupedByUser);
        Assert.Equal(1, actualDto.GroupedByUser.Count);
        Assert.All(actualDto.GroupedByUser, item =>
        {
            Assert.Equal(_user.Id, item.UserId);
            Assert.Equal(_user.UserName, item.UserName);
            Assert.Equal(TimeSpan.FromHours(15), item.Duration);
        });
    }
}
