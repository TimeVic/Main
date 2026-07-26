using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Project.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, ProjectDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IClientDao _clientDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IClientDao clientDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _clientDao = clientDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
        }
    
        public async Task<ProjectDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();

            var project = await _projectDao.GetById(request.ProjectId);
            RecordNotFoundException.ThrowIfNull(project);
            
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }
            var client = await _clientDao.GetById(request.ClientId, project.Client.Workspace);
            RecordNotFoundException.ThrowIfNull(client);
            project.SetClient(client);
            project = _mapper.Map(request, project);
            await _sessionProvider.CurrentSession.SaveAsync(project);
            
            return _mapper.Map<ProjectDto>(project);
        }
    }
}
