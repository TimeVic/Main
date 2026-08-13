using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMember.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, WorkspaceMemberDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly IWorkspaceDao _workspaceDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _workspaceDao = workspaceDao;
        }
    
        public async Task<WorkspaceMemberDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _apiRequestService.GetCurrentUserId();
            var user = await _userDao.GetById(userId);
            var member = await _workspaceDao.GetMemberAsync(request.MemberId);
            if (member?.Workspace.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Write, user!, member.Workspace))
            {
                throw new HasNoAccessException();
            }

            var projects = new List<ProjectEntity>();
            if (request.ProjectsAccess.Any())
            {
                projects = member.Workspace.Clients
                    .SelectMany(item => item.Projects)
                    .Where(
                        item => request.ProjectsAccess.Any(
                            projectAccessDto => projectAccessDto.ProjectId == item.Id && projectAccessDto.HasAccess
                        )
                    )
                    .ToList();
            }
            // TODO: Should be refactored to Manager role. In this case HourlyRate will not be saved if HasAccess = false
            var workspaceMember = await _workspaceAccessService.ShareAccessAsync(
                member.Workspace,
                member.User,
                request.Access,
                projects
                    .Where(
                        project => request.ProjectsAccess.Any(
                            item => item.ProjectId == project.Id
                        )
                    )
                    .Select(
                        item =>
                        {
                            var providedAccess = request.ProjectsAccess.FirstOrDefault(
                                projectAccess => projectAccess.ProjectId == item.Id && projectAccess.HasAccess
                            );
                            return new ProjectAccessModel()
                            {
                                Project = providedAccess != null ? item : null!,
                                HourlyRate = providedAccess?.HourlyRate
                            };
                        }
                    ).ToList()
            );
            return _mapper.Map<WorkspaceMemberDto>(workspaceMember);
        }
    }
}
