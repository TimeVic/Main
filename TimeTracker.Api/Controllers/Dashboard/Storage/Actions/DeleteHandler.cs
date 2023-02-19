using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class DeleteHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IFileStorage _fileStorage;

        public DeleteHandler(
            IRequestService requestService,
            IUserDao userDao,
            IFileStorage fileStorage
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _fileStorage = fileStorage;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            await _fileStorage.DeleteFile(user, request.Id);
        }
    }
}
