using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Services.Report;

public class SharedClientReportService : ISharedClientReportService
{
    private readonly ISharedClientReportDao _sharedClientReportDao;
    private readonly IWorkspaceFinancialSummaryReportDao _reportDao;

    public SharedClientReportService(
        ISharedClientReportDao sharedClientReportDao,
        IWorkspaceFinancialSummaryReportDao reportDao
    )
    {
        _sharedClientReportDao = sharedClientReportDao;
        _reportDao = reportDao;
    }

    public async Task<SharedClientReportEntity> GetActiveAsync(string token, bool isRequireTasks = false)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new PublicSharedReportNotFoundException();
        }

        var report = await _sharedClientReportDao.GetByTokenAsync(token);
        if (report == null || !report.IsActive || (isRequireTasks && !report.IsShowTasks))
        {
            throw new PublicSharedReportNotFoundException();
        }

        return report;
    }

    public async Task<GetSharedClientReportResponse> GetReportAsync(SharedClientReportEntity report)
    {
        var projects = await _reportDao.GetSharedClientReportProjectsAsync(report.Client.Id);
        var payments = await _reportDao.GetSharedClientReportPaymentsAsync(report.Client.Id);
        var projectDtos = projects.Select(item => new SharedClientReportProjectDto
        {
            Id = item.ProjectId,
            Name = item.ProjectName,
            Duration = item.Duration,
            Earned = item.Earned
        }).ToList();
        var paymentDtos = payments.Select(item => new SharedClientReportPaymentDto
        {
            PaymentTime = item.PaymentTime,
            Amount = item.Amount,
            ProjectName = item.ProjectName,
            Description = item.Description
        }).ToList();

        return new GetSharedClientReportResponse
        {
            ClientName = report.Client.Name,
            WorkspaceName = report.Client.Workspace.Name,
            CurrencyCode = report.Client.Workspace.Currency?.Code,
            IsShowTasks = report.IsShowTasks,
            Projects = projectDtos,
            Payments = paymentDtos,
            Totals = new SharedClientReportTotalsDto
            {
                Earned = projectDtos.Sum(item => item.Earned),
                Received = paymentDtos.Sum(item => item.Amount)
            }
        };
    }

    public async Task<GetSharedClientReportTasksResponse> GetTasksAsync(SharedClientReportEntity report)
    {
        var tasks = await _reportDao.GetSharedClientReportTasksAsync(report.Client.Id);
        return new GetSharedClientReportTasksResponse
        {
            Tasks = tasks.Select(item => new SharedClientReportTaskDto
            {
                Id = item.TaskId,
                ProjectId = item.ProjectId,
                Title = item.TaskTitle,
                Duration = item.Duration
            }).ToList()
        };
    }
}
