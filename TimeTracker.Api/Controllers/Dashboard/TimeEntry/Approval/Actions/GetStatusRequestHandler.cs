using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Approval.Actions;

public class GetStatusRequestHandler : IAsyncRequestHandler<GetStatusRequest, TimeEntryApprovalStatusSummaryDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly ITimeEntryApprovalService _approvalService;

    public GetStatusRequestHandler(
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

    public async Task<TimeEntryApprovalStatusSummaryDto> ExecuteAsync(GetStatusRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        return await _approvalService.GetStatusSummaryAsync(user, workspace);
    }
}
