using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class GetListHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskDao _taskDao;

        public GetListHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            ISecurityManager securityManager,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _securityManager = securityManager;
            _taskDao = taskDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();

            if (request.EntityType == StorageEntityType.Task)
            {
                var task = await _taskDao.GetById(request.EntityId);
                RecordNotFoundException.ThrowIfNull(task);
                if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                {
                    throw new HasNoAccessException();
                }
                return new GetListResponse(
                    _mapper.Map<ICollection<StoredFileDto>>(task.Attachments),
                    task.Attachments.Count
                );
            }
            throw new ValidationException("Incorrect entity type");
        }
    }
}
