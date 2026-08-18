using TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Services.Report;

public class SharedClientReportService : ISharedClientReportService
{
    private readonly ISharedClientReportDao _sharedClientReportDao;
    private readonly IClientFinancialReportDataDao _reportDataDao;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SharedClientReportService(
        ISharedClientReportDao sharedClientReportDao,
        IClientFinancialReportDataDao reportDataDao,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _sharedClientReportDao = sharedClientReportDao;
        _reportDataDao = reportDataDao;
        _httpContextAccessor = httpContextAccessor;
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
        var projects = await _reportDataDao.GetProjectBreakdownAsync(report.Client);
        var projectDtos = projects
            .Select(item => new SharedClientReportProjectDto
            {
                Id = item.ProjectId,
                Name = item.ProjectName,
                Duration = item.Duration,
                Earned = item.EarnedAmount
            })
            .ToList();
        var paymentDtos = (await _reportDataDao.GetPaymentsAsync(report.Client))
            .Select(item => new SharedClientReportPaymentDto
            {
                PaymentTime = item.PaymentTime,
                Amount = item.Amount,
                ProjectName = item.ProjectName,
                Description = item.Description
            })
            .ToList();

        return new GetSharedClientReportResponse
        {
            ClientName = report.Client.Name,
            WorkspaceName = report.Client.Workspace.Name,
            CultureCode = GetCultureCode(report),
            CurrencyCode = report.Client.Workspace.Currency.Code,
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

    private string GetCultureCode(SharedClientReportEntity report)
    {
        var browserCulture = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
        var workspaceOwnerCulture = report.Client.Workspace.CreatedUser.Language.Code;
        return CultureCodeHelper.GetSupportedCultureCode(browserCulture)
            ?? CultureCodeHelper.GetSupportedCultureCode(workspaceOwnerCulture)
            ?? CultureCodeHelper.EnglishCultureCode;
    }

    public async Task<GetSharedClientReportTasksResponse> GetTasksAsync(SharedClientReportEntity report, Guid projectId, int page)
    {
        var project = report.Client.Projects.FirstOrDefault(item => item.Id == projectId);
        RecordNotFoundException.ThrowIfNull(project);
        var taskItems = await _reportDataDao.GetTasksAsync(project, page);

        return new GetSharedClientReportTasksResponse
        {
            Tasks = taskItems.Items
                .Select(item => new SharedClientReportTaskDto
                {
                    Id = item.Id,
                    ProjectId = item.ProjectId,
                    Title = item.Title,
                    Duration = item.Duration
                })
                .ToList(),
            TotalCount = taskItems.TotalCount,
            PageSize = PaginationUtils.DefaultPageSize,
            TotalPages = PaginationUtils.CalculateTotalPages(taskItems.TotalCount),
            IsHasMore = page * PaginationUtils.DefaultPageSize < taskItems.TotalCount
        };
    }
}
