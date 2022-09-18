using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions
{
    public class PaymentReportRequestHandler : IAsyncRequestHandler<PaymentReportRequest, PaymentReportResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryReportsDao _entryReportsDao;

        public PaymentReportRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITimeEntryReportsDao entryReportsDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _entryReportsDao = entryReportsDao;
        }
    
        public async Task<PaymentReportResponse> ExecuteAsync(PaymentReportRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.Workspaces.FirstOrDefault(item => item.Id == request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException("Workspace not found");
            }

            var reportItems = await _entryReportsDao.GetProjectPaymentsReport(workspace.Id);
            return new PaymentReportResponse()
            {
                Items = _mapper.Map<ICollection<PaymentsReportItemDto>>(reportItems)
            };
        }
    }
}
