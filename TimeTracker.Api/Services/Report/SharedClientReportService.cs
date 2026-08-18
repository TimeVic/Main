using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Services.Report;

public class SharedClientReportService : ISharedClientReportService
{
    private readonly ISharedClientReportDao _sharedClientReportDao;
    private readonly IWorkspaceFinancialSummaryReportDao _reportDao;
    private readonly ITaskDao _taskDao;

    public SharedClientReportService(
        ISharedClientReportDao sharedClientReportDao,
        IWorkspaceFinancialSummaryReportDao reportDao,
        ITaskDao taskDao
    )
    {
        _sharedClientReportDao = sharedClientReportDao;
        _reportDao = reportDao;
        _taskDao = taskDao;
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
        var projects = await _reportDao.GetClientProjectBreakdownAsync(report.Client.Workspace.Id);
        var projectDtos = projects
            .Where(item => item.ClientId == report.Client.Id)
            .Select(item => new SharedClientReportProjectDto
        {
            Id = item.ProjectId,
            Name = item.ProjectName,
            Duration = item.Duration,
            Earned = item.EarnedAmount
        }).ToList();
        var paymentDtos = report.Client.ClientPayments
            .OrderByDescending(item => item.PaymentTime)
            .ThenByDescending(item => item.CreatedAt)
            .Select(item => new SharedClientReportPaymentDto
        {
            PaymentTime = item.PaymentTime,
            Amount = item.Amount,
            ProjectName = item.Project?.Name,
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
        var taskLists = report.Client.Projects
            .SelectMany(project => project.TaskLists)
            .ToList();
        var tasks = new List<TaskEntity>();
        foreach (var taskList in taskLists)
        {
            var taskListTasks = await _taskDao.GetList(taskList: taskList);
            tasks.AddRange(taskListTasks.Items);
        }
        var durationsByTaskId = await _taskDao.GetTrackedDurationByTaskIds(tasks.Select(item => item.Id).ToList());

        return new GetSharedClientReportTasksResponse
        {
            Tasks = tasks
                .Where(item => durationsByTaskId.TryGetValue(item.Id, out var duration) && duration > TimeSpan.Zero)
                .Select(item => new SharedClientReportTaskDto
                {
                    Id = item.Id,
                    ProjectId = item.TaskList.Project.Id,
                    Title = item.Title,
                    Duration = durationsByTaskId[item.Id]
                })
                .OrderByDescending(item => item.Duration)
                .ThenBy(item => item.Title)
                .ToList()
        };
    }
}
