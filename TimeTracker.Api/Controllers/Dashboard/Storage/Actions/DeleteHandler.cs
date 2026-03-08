using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class DeleteHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IFileStorage _fileStorage;

        public DeleteHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IFileStorage fileStorage
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _fileStorage = fileStorage;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            RecordNotFoundException.ThrowIfNull(user);
            await _fileStorage.DeleteFile(user, request.Id);
        }
    }
}
