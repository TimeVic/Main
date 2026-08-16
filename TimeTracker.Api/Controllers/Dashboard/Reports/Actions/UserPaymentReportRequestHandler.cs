using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.UserPaymentReport;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions;

public class UserPaymentReportRequestHandler : IAsyncRequestHandler<UserPaymentReportRequest, UserPaymentReportResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceFinancialSummaryReportDao _reportDao;
    private readonly ITimeEntryReportsDao _timeEntryReportsDao;

    public UserPaymentReportRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        IWorkspaceFinancialSummaryReportDao reportDao,
        ITimeEntryReportsDao timeEntryReportsDao
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _reportDao = reportDao;
        _timeEntryReportsDao = timeEntryReportsDao;
    }

    public async Task<UserPaymentReportResponse> ExecuteAsync(UserPaymentReportRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace);

        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var endDate = (request.EndDate == default ? DateTime.UtcNow : request.EndDate).EndOfDay();
        if (workspace.Mode == WorkspaceMode.Team)
        {
            return await GetTeamWorkspaceReportAsync(workspace.Id, user.Id, endDate);
        }

        return await GetSoloWorkspaceReportAsync(workspace.Id, user.Id, endDate);
    }

    private async Task<UserPaymentReportResponse> GetSoloWorkspaceReportAsync(Guid workspaceId, Guid userId, DateTime endDate)
    {
        var projectEarnings = await _reportDao.GetUserPaymentReportProjectEarningsAsync(workspaceId, userId, endDate);
        var clientPayments = await _reportDao.GetUserPaymentReportClientPaymentsAsync(workspaceId, endDate);

        var clients = projectEarnings
            .GroupBy(item => new { item.ClientId, item.ClientName })
            .Select(group => new UserPaymentReportClientDto
            {
                Id = group.Key.ClientId,
                Name = group.Key.ClientName,
                Duration = group.Aggregate(TimeSpan.Zero, (total, item) => total + item.Duration),
                Earned = group.Sum(item => item.Earned),
                Projects = group.Select(item => new UserPaymentReportProjectDto
                {
                    Id = item.ProjectId,
                    Name = item.ProjectName,
                    Duration = item.Duration,
                    Earned = item.Earned
                }).ToList()
            })
            .ToDictionary(item => item.Id);

        foreach (var payment in clientPayments)
        {
            if (!clients.TryGetValue(payment.ClientId, out var client))
            {
                client = new UserPaymentReportClientDto
                {
                    Id = payment.ClientId,
                    Name = payment.ClientName
                };
                clients.Add(client.Id, client);
            }

            client.ProjectPayments = payment.ProjectPayments;
            client.GeneralPayments = payment.GeneralPayments;
        }

        var clientItems = clients.Values.OrderBy(item => item.Name).ToList();
        return new UserPaymentReportResponse
        {
            IsPaymentsFromMembers = false,
            Clients = clientItems,
            Totals = new UserPaymentReportTotalsDto
            {
                Earned = clientItems.Sum(item => item.Earned),
                Received = clientItems.Sum(item => item.Received)
            }
        };
    }

    private async Task<UserPaymentReportResponse> GetTeamWorkspaceReportAsync(Guid workspaceId, Guid userId, DateTime endDate)
    {
        var reportItems = await _timeEntryReportsDao.GetProjectMemberPaymentsReport(workspaceId, userId, endDate);
        var clients = reportItems
            .GroupBy(item => new { item.ClientId, item.ClientName })
            .Select(group => new UserPaymentReportClientDto
            {
                Id = group.Key.ClientId ?? Guid.Empty,
                Name = group.Key.ClientName ?? string.Empty,
                Duration = group.Aggregate(TimeSpan.Zero, (total, item) => total + item.TotalDuration),
                Earned = group.Sum(item => item.Amount),
                ProjectPayments = group.First().PaidAmountByClient,
                Projects = group.Select(item => new UserPaymentReportProjectDto
                {
                    Id = item.ProjectId ?? Guid.Empty,
                    Name = item.ProjectName ?? string.Empty,
                    Duration = item.TotalDuration,
                    Earned = item.Amount
                }).ToList()
            })
            .OrderBy(item => item.Name)
            .ToList();

        return new UserPaymentReportResponse
        {
            IsPaymentsFromMembers = true,
            Clients = clients,
            Totals = new UserPaymentReportTotalsDto
            {
                Earned = clients.Sum(item => item.Earned),
                Received = clients.Sum(item => item.Received)
            }
        };
    }
}
