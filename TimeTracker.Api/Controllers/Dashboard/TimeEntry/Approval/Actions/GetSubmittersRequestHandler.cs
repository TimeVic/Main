using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Approval.Actions;

public class GetSubmittersRequestHandler : IAsyncRequestHandler<GetSubmittersRequest, GetSubmittersResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly ITimeEntryApprovalService _approvalService;

    public GetSubmittersRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        ITimeEntryApprovalService approvalService
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _approvalService = approvalService;
    }

    public async Task<GetSubmittersResponse> ExecuteAsync(GetSubmittersRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        var rawItems = await _approvalService.GetSubmittersAsync(user, workspace);

        var items = rawItems.Select(item => new TimeEntryApprovalSubmitterDto
        {
            UserId = item.UserId,
            UserName = string.IsNullOrWhiteSpace(item.UserName) ? item.Login : item.UserName,
            Login = item.Login,
            PeriodStartDate = item.PeriodStartDate,
            PeriodEndDate = item.PeriodEndDate,
            WeekNumber = item.PeriodStartDate.GetIso8601WeekOfYear(),
            TotalDuration = TimeSpan.FromSeconds(item.TotalDurationSeconds),
            TotalDeveloperAmount = item.TotalDeveloperAmount,
            TotalClientAmount = item.TotalClientAmount,
            PendingCount = item.PendingCount,
            Status = TimeEntryStatus.Pending,
            IsCurrentUser = item.UserId == user.Id
        }).ToList();

        return new GetSubmittersResponse
        {
            Items = items
        };
    }
}
