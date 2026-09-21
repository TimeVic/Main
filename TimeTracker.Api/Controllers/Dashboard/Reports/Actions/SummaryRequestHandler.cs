using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions
{
    public class SummaryRequestHandler : IAsyncRequestHandler<SummaryReportRequest, SummaryReportResponse>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ISummaryReportService _summaryReportService;

        public SummaryRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ISummaryReportService summaryReportService
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _summaryReportService = summaryReportService;
        }
    
        public async Task<SummaryReportResponse> ExecuteAsync(SummaryReportRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            return await _summaryReportService.GetReportAsync(
                user,
                workspace!.Id,
                request.StartTime,
                request.EndTime,
                request.Type
            );
        }
    }
}
