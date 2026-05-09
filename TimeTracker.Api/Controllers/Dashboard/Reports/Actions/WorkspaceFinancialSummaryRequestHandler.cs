using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
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
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IWorkspaceFinancialSummaryReportDao _reportDao;
    private readonly IWorkspaceDao _workspaceDao;

    public WorkspaceFinancialSummaryRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService,
        IWorkspaceFinancialSummaryReportDao reportDao,
        IWorkspaceDao workspaceDao
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
        _reportDao = reportDao;
        _workspaceDao = workspaceDao;
    }

    public async Task<WorkspaceFinancialSummaryReportResponse> ExecuteAsync(
        WorkspaceFinancialSummaryReportRequest request
    )
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace);

        if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var clientBalances = await _reportDao.GetClientBalancesAsync(workspace.Id);
        var memberBalances = await _reportDao.GetMemberBalancesAsync(workspace.Id);
        var projectProfitability = await _reportDao.GetProjectProfitabilityAsync(workspace.Id);

        var membersPage = await _workspaceDao.GetMembersAsync(workspace, 1);
        var isTeamWorkspace = membersPage.TotalCount > 1;

        var totals = BuildTotals(clientBalances, memberBalances);

        return new WorkspaceFinancialSummaryReportResponse
        {
            IsTeamWorkspace = isTeamWorkspace,
            HasMemberPayouts = isTeamWorkspace || memberBalances.Any(x => x.PaidOutAmount != 0),
            HasUsefulProjectProfitability = isTeamWorkspace || projectProfitability.Any(x => x.EstimatedMargin != 0),
            Totals = totals,
            ClientBalances = MapClientBalances(clientBalances),
            MemberBalances = MapMemberBalances(memberBalances),
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
            RealizedMargin = clientReceived - memberPaidOut
        };
    }

    private ICollection<WorkspaceFinancialClientBalanceDto> MapClientBalances(
        ICollection<FinancialClientBalanceItemDto> items
    )
    {
        return items.Select(item => new WorkspaceFinancialClientBalanceDto
        {
            Client = new ClientDto { Id = item.ClientId, Name = item.ClientName },
            Duration = item.Duration,
            Earned = item.EarnedAmount,
            Received = item.ReceivedAmount,
            Outstanding = item.OutstandingAmount,
            LastPaymentDate = item.LastPaymentDate
        }).ToList();
    }

    private ICollection<WorkspaceFinancialMemberBalanceDto> MapMemberBalances(
        ICollection<FinancialMemberBalanceItemDto> items
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
            LastPayoutDate = item.LastPayoutDate
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
            EstimatedMargin = item.EstimatedMargin,
            MarginPercent = item.MarginPercent
        }).ToList();
    }
}
