using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker.GoalsTrackerItemController.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteItemRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

        public DeleteRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IGoalsTrackerItemsDao goalsTrackerItemsDao
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _goalsTrackerItemsDao = goalsTrackerItemsDao;
        }
    
        public async Task ExecuteAsync(DeleteItemRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var goalsTrackerItem = await _goalsTrackerItemsDao.GetById(request.Id);
            await _securityManager.CheckAccess(AccessLevel.Write, user, goalsTrackerItem?.Tracker);
            await _goalsTrackerItemsDao.Archive(goalsTrackerItem);
        }
    }
}
