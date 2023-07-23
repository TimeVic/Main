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
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IDbSessionProvider sessionProvider,
            IProjectDao projectDao,
            ISecurityManager securityManager,
            IRequestService requestService,
            IUserDao userDao
        )
        {
            _sessionProvider = sessionProvider;
            _projectDao = projectDao;
            _securityManager = securityManager;
            _requestService = requestService;
            _userDao = userDao;
        }

        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            var project = await _projectDao.GetById(request.ProjectId, true);
            if (project == null)
            {
                throw new RecordNotFoundException();
            }

            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }

            await _projectDao.ArchiveProject(project);
        }
    }
}
