using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions;

public class TeamSummaryRequestHandler : IAsyncRequestHandler<TeamSummaryReportRequest, TeamSummaryReportResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly ISummaryReportDao _summaryReportDao;

    public TeamSummaryRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        ISummaryReportDao summaryReportDao
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _summaryReportDao = summaryReportDao;
    }

    public async Task<TeamSummaryReportResponse> ExecuteAsync(TeamSummaryReportRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace);

        if (workspace.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new ForbiddenException();
        }

        var byDays = await _summaryReportDao.GetTeamReportByDayAsync(workspace.Id, request.StartTime, request.EndTime);
        var members = await _summaryReportDao.GetTeamReportByMemberAsync(workspace.Id, request.StartTime, request.EndTime);

        return new TeamSummaryReportResponse
        {
            Totals = new TeamSummaryTotalsDto
            {
                Duration = new TimeSpan(byDays.Sum(item => item.Duration.Ticks)),
                ClientBillable = byDays.Sum(item => item.ClientBillable),
                TeamLaborCost = byDays.Sum(item => item.TeamLaborCost)
            },
            ByDays = byDays.Select(item => new TeamSummaryByDaysReportItemDto
            {
                Date = item.Date,
                Duration = item.Duration,
                ClientBillable = item.ClientBillable,
                TeamLaborCost = item.TeamLaborCost
            }).ToList(),
            Members = members.Select(item => new TeamSummaryMemberReportItemDto
            {
                UserName = item.UserName,
                Email = item.Email,
                Duration = item.Duration,
                ClientBillable = item.ClientBillable,
                TeamLaborCost = item.TeamLaborCost
            }).ToList()
        };
    }
}
