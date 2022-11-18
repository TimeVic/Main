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
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Reports.Actions
{
    public class SummaryRequestHandler : IAsyncRequestHandler<SummaryReportRequest, SummaryReportResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ISummaryReportDao _summaryReportDao;
        private readonly IProjectDao _projectDao;

        public SummaryRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ISummaryReportDao summaryReportDao,
            IProjectDao projectDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _summaryReportDao = summaryReportDao;
            _projectDao = projectDao;
        }
    
        public async Task<SummaryReportResponse> ExecuteAsync(SummaryReportRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            if (accessType is MembershipAccessType.Manager or MembershipAccessType.Owner)
            {
                return await GetReportForOwnerOrManagerAsync(
                    workspace,
                    request.StartTime,
                    request.EndTime,
                    request.Type
                );
            }
            var availableProjects = await _projectDao.GetAvailableForUserListAsync(
                workspace,
                user,
                accessType
            );
            return await GetReportForOtherAsync(
                user,
                availableProjects.Items,
                request.StartTime,
                request.EndTime,
                request.Type
            );
        }

        private async Task<SummaryReportResponse> GetReportForOwnerOrManagerAsync(
            WorkspaceEntity workspace,
            DateTime startTime,
            DateTime endTime,
            SummaryReportType type
        )
        {
            var byDaysReportItems = await _summaryReportDao.GetReportByDayForOwnerOrManagerAsync(
                workspace.Id,
                startTime,
                endTime
            );
            var response = new SummaryReportResponse()
            {
                ByDays = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(byDaysReportItems)
            };
            if (type == SummaryReportType.GroupByDay)
            {
                var groupedItems = await _summaryReportDao.GetReportByDayForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByDay = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByClient)
            {
                var groupedItems = await _summaryReportDao.GetReportByClientForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByClient = _mapper.Map<ICollection<SummaryByClientsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByProject)
            {
                var groupedItems = await _summaryReportDao.GetReportByProjectForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByProject = _mapper.Map<ICollection<SummaryByProjectsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByMonth)
            {
                var groupedItems = await _summaryReportDao.GetReportByMonthForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByMonth = _mapper.Map<ICollection<SummaryByMonthsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByWeek)
            {
                var groupedItems = await _summaryReportDao.GetReportByWeekForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByWeek = _mapper.Map<ICollection<SummaryByWeeksReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByUser)
            {
                var groupedItems = await _summaryReportDao.GetReportByUserForOwnerOrManagerAsync(
                    workspace.Id,
                    startTime,
                    endTime
                );
                response.GroupedByUser = _mapper.Map<ICollection<SummaryByUsersReportItemDto>>(groupedItems);
            }

            return response;
        }
        
        private async Task<SummaryReportResponse> GetReportForOtherAsync(
            UserEntity currentUser,
            ICollection<ProjectEntity> availableProjectsForUser,
            DateTime startTime,
            DateTime endTime,
            SummaryReportType type
        )
        {
            var byDaysReportItems = await _summaryReportDao.GetReportByDayForOtherAsync(
                startTime,
                endTime,
                currentUser.Id,
                availableProjectsForUser
            );
            var response = new SummaryReportResponse()
            {
                ByDays = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(byDaysReportItems)
            };
            if (type == SummaryReportType.GroupByDay)
            {
                var groupedItems = await _summaryReportDao.GetReportByDayForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByDay = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByClient)
            {
                var groupedItems = await _summaryReportDao.GetReportByClientForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByClient = _mapper.Map<ICollection<SummaryByClientsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByProject)
            {
                var groupedItems = await _summaryReportDao.GetReportByProjectForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByProject = _mapper.Map<ICollection<SummaryByProjectsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByMonth)
            {
                var groupedItems = await _summaryReportDao.GetReportByMonthForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByMonth = _mapper.Map<ICollection<SummaryByMonthsReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByWeek)
            {
                var groupedItems = await _summaryReportDao.GetReportByWeekForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByWeek = _mapper.Map<ICollection<SummaryByWeeksReportItemDto>>(groupedItems);
            }
            else if (type == SummaryReportType.GroupByUser)
            {
                var groupedItems = await _summaryReportDao.GetReportByUserForOtherAsync(
                    startTime,
                    endTime,
                    currentUser.Id,
                    availableProjectsForUser
                );
                response.GroupedByUser = _mapper.Map<ICollection<SummaryByUsersReportItemDto>>(groupedItems);
            }

            return response;
        }
    }
}
