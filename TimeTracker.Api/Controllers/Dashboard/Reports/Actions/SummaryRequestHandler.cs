using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions
{
    public class SummaryRequestHandler : IAsyncRequestHandler<SummaryReportRequest, SummaryReportResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ISummaryReportDao _summaryReportDao;

        public SummaryRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ISummaryReportDao summaryReportDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _summaryReportDao = summaryReportDao;
        }
    
        public async Task<SummaryReportResponse> ExecuteAsync(SummaryReportRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            return await GetReportAsync(
                user,
                workspace!.Id,
                request.StartTime,
                request.EndTime,
                request.Type
            );
        }

        private async Task<SummaryReportResponse> GetReportAsync(
            UserEntity currentUser,
            Guid workspaceId,
            DateTime startTime,
            DateTime endTime,
            SummaryReportType type
        )
        {
            var byDaysReportItems = await _summaryReportDao.GetReportByDayAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            var response = new SummaryReportResponse()
            {
                ByDays = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(byDaysReportItems)
            };
            if (type == SummaryReportType.GroupByDay)
            {
                var groupedItems = await _summaryReportDao.GetReportByDayAsync(
                    workspaceId,
                    currentUser.Id,
                    startTime,
                    endTime
                );
                response.GroupedByDay = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByClient)
            {
                var groupedItems = await _summaryReportDao.GetReportByClientAsync(
                    workspaceId,
                    currentUser.Id,
                    startTime,
                    endTime
                );
                response.GroupedByClient = _mapper.Map<ICollection<SummaryByClientsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByProject)
            {
                var groupedItems = await _summaryReportDao.GetReportByProjectAsync(
                    workspaceId,
                    currentUser.Id,
                    startTime,
                    endTime
                );
                response.GroupedByProject = _mapper.Map<ICollection<SummaryByProjectsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByMonth)
            {
                var groupedItems = await _summaryReportDao.GetReportByMonthAsync(
                    workspaceId,
                    currentUser.Id,
                    startTime,
                    endTime
                );
                response.GroupedByMonth = _mapper.Map<ICollection<SummaryByMonthsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByWeek)
            {
                var groupedItems = await _summaryReportDao.GetReportByWeekAsync(
                    workspaceId,
                    currentUser.Id,
                    startTime,
                    endTime
                );
                response.GroupedByWeek = _mapper.Map<ICollection<SummaryByWeeksReportItemDto>>(groupedItems);
            }
            return response;
        }
    }
}
