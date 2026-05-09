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
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, ProjectDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IClientDao _clientDao;
        private readonly IProjectDao _projectDao;
        private readonly ISecurityManager _securityManager;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IClientDao clientDao,
            IProjectDao projectDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _clientDao = clientDao;
            _projectDao = projectDao;
            _securityManager = securityManager;
        }
    
        public async Task<ProjectDto> ExecuteAsync(AddRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var client = await _clientDao.GetById(request.ClientId);
            RecordNotFoundException.ThrowIfNull(client);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, client.Workspace))
            {
                throw new HasNoAccessException();
            }

            var project = await _projectDao.CreateAsync(client, request.Name);

            return _mapper.Map<ProjectDto>(project);
        }
    }
}
