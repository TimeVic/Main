using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions
{
    public class MemberPaymentReportRequestHandler : IAsyncRequestHandler<MemberPaymentReportRequest, MemberPaymentReportResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryReportsDao _entryReportsDao;
        private readonly ISecurityManager _securityManager;

        public MemberPaymentReportRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryReportsDao entryReportsDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _entryReportsDao = entryReportsDao;
            _securityManager = securityManager;
        }
    
        public async Task<MemberPaymentReportResponse> ExecuteAsync(MemberPaymentReportRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            RecordNotFoundException.ThrowIfNull(workspace);
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var reportItems = await _entryReportsDao.GetProjectMemberPaymentsReport(
                workspace.Id,
                user.Id,
                request.EndDate
            );
            return new MemberPaymentReportResponse()
            {
                Items = _mapper.Map<ICollection<MemberPaymentsReportItemDto>>(reportItems)
            };
        }
    }
}
