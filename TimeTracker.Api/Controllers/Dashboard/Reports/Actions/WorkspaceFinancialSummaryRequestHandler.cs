using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions;

public class WorkspaceFinancialSummaryRequestHandler
    : IAsyncRequestHandler<WorkspaceFinancialSummaryReportRequest, WorkspaceFinancialSummaryReportResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceFinancialSummaryReportDao _reportDao;

    public WorkspaceFinancialSummaryRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        IWorkspaceFinancialSummaryReportDao reportDao
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _reportDao = reportDao;
    }

    public async Task<WorkspaceFinancialSummaryReportResponse> ExecuteAsync(
        WorkspaceFinancialSummaryReportRequest request
    )
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace);

        if (workspace.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var clientBalances = await _reportDao.GetClientBalancesAsync(workspace.Id);
        var clientProjects = await _reportDao.GetClientProjectBreakdownAsync(workspace.Id);
        var memberBalances = await _reportDao.GetMemberBalancesAsync(workspace.Id);
        var memberProjects = await _reportDao.GetMemberProjectBreakdownAsync(workspace.Id);
        var projectProfitability = await _reportDao.GetProjectProfitabilityAsync(workspace.Id);
        var totals = BuildTotals(clientBalances, memberBalances);

        return new WorkspaceFinancialSummaryReportResponse
        {
            IsTeamWorkspace = true,
            HasMemberPayouts = true,
            HasUsefulProjectProfitability = true,
            Totals = totals,
            ClientBalances = MapClientBalances(clientBalances, clientProjects),
            MemberBalances = MapMemberBalances(memberBalances, memberProjects),
            ProjectProfitability = MapProjectProfitability(projectProfitability)
        };
    }

    private static WorkspaceFinancialSummaryTotalsDto BuildTotals(
        ICollection<FinancialClientBalanceItemDto> clientBalances,
        ICollection<FinancialMemberBalanceItemDto> memberBalances
    )
    {
        var clientEarned = clientBalances.Sum(x => x.EarnedAmount);
        var clientReceived = clientBalances.Sum(x => x.ReceivedAmount);
        var teamCost = memberBalances.Sum(x => x.CostAmount);
        var memberPaidOut = memberBalances.Sum(x => x.PaidOutAmount);

        return new WorkspaceFinancialSummaryTotalsDto
        {
            ClientEarned = clientEarned,
            ClientReceived = clientReceived,
            ClientOutstanding = clientEarned - clientReceived,
            TeamCost = teamCost,
            MemberPaidOut = memberPaidOut,
            MemberOutstanding = teamCost - memberPaidOut,
            EstimatedMargin = clientEarned - teamCost,
            RealizedMargin = clientReceived - memberPaidOut,
            MarginPercent = clientEarned == 0 ? null : Math.Round((clientEarned - teamCost) / clientEarned * 100, 1)
        };
    }

    private ICollection<WorkspaceFinancialClientBalanceDto> MapClientBalances(
        ICollection<FinancialClientBalanceItemDto> items,
        ICollection<FinancialClientProjectItemDto> projects
    )
    {
        return items.Select(item => new WorkspaceFinancialClientBalanceDto
        {
            Client = new ClientDto { Id = item.ClientId, Name = item.ClientName },
            Duration = item.Duration,
            Earned = item.EarnedAmount,
            Received = item.ReceivedAmount,
            Outstanding = item.OutstandingAmount,
            LastPaymentDate = item.LastPaymentDate,
            Projects = projects
                .Where(project => project.ClientId == item.ClientId)
                .Select(project => new WorkspaceFinancialClientProjectDto
                {
                    Project = new ProjectDto { Id = project.ProjectId, Name = project.ProjectName },
                    Duration = project.Duration,
                    Earned = project.EarnedAmount
                })
                .ToList()
        }).ToList();
    }

    private ICollection<WorkspaceFinancialMemberBalanceDto> MapMemberBalances(
        ICollection<FinancialMemberBalanceItemDto> items,
        ICollection<FinancialMemberProjectItemDto> projects
    )
    {
        return items.Select(item => new WorkspaceFinancialMemberBalanceDto
        {
            MemberId = item.MemberId,
            User = new UserDto { Id = item.UserId, UserName = item.UserName, Email = item.Email },
            Duration = item.Duration,
            Cost = item.CostAmount,
            PaidOut = item.PaidOutAmount,
            Owed = item.OwedAmount,
            LastPayoutDate = item.LastPayoutDate,
            Projects = projects
                .Where(project => project.MemberId == item.MemberId)
                .Select(project => new WorkspaceFinancialMemberProjectDto
                {
                    Project = new ProjectDto { Id = project.ProjectId, Name = project.ProjectName },
                    Client = project.ClientId.HasValue
                        ? new ClientDto { Id = project.ClientId.Value, Name = project.ClientName ?? string.Empty }
                        : null,
                    Duration = project.Duration,
                    Earned = project.CostAmount
                })
                .ToList()
        }).ToList();
    }

    private ICollection<WorkspaceFinancialProjectProfitabilityDto> MapProjectProfitability(
        ICollection<FinancialProjectProfitabilityItemDto> items
    )
    {
        return items.Select(item => new WorkspaceFinancialProjectProfitabilityDto
        {
            Project = new ProjectDto { Id = item.ProjectId, Name = item.ProjectName },
            Client = item.ClientId.HasValue
                ? new ClientDto { Id = item.ClientId.Value, Name = item.ClientName ?? string.Empty }
                : null,
            Duration = item.Duration,
            ClientEarned = item.EarnedAmount,
            TeamCost = item.TeamCostAmount,
            ClientHourlyRate = item.ClientHourlyRate,
            TeamHourlyRate = item.TeamHourlyRate,
            EstimatedMargin = item.EstimatedMargin,
            MarginPercent = item.MarginPercent
        }).ToList();
    }
}
