using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Project.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, ProjectDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
        }
    
        public async Task<ProjectDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            var project = await _projectDao.GetById(request.ProjectId);
            if (project == null || !await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }
            var client = project.Workspace.Clients.FirstOrDefault(item => item.Id == request.ClientId);
            project.SetClient(client);
            project = _mapper.Map(request, project);
            await _sessionProvider.CurrentSession.SaveAsync(project);
            
            return _mapper.Map<ProjectDto>(project);
        }
    }
}
