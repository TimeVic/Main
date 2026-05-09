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
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IDbSessionProvider sessionProvider,
            IProjectDao projectDao,
            ISecurityManager securityManager,
            IApiRequestService apiRequestService,
            IUserDao userDao
        )
        {
            _sessionProvider = sessionProvider;
            _projectDao = projectDao;
            _securityManager = securityManager;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
        }

        public async Task ExecuteAsync(DeleteRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();

            var project = await _projectDao.GetById(request.ProjectId, true);
            RecordNotFoundException.ThrowIfNull(project);

            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }

            await _projectDao.ArchiveProject(project);
        }
    }
}
