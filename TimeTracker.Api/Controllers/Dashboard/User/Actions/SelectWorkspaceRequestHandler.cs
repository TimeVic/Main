using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class SelectWorkspaceRequestHandler : IAsyncRequestHandler<SelectWorkspaceRequest, UserDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ISecurityManager _securityManager;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public SelectWorkspaceRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IWorkspaceDao workspaceDao,
        ISecurityManager securityManager,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _workspaceDao = workspaceDao;
        _securityManager = securityManager;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<UserDto> ExecuteAsync(SelectWorkspaceRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _workspaceDao.GetById(request.WorkspaceId);
        RecordNotFoundException.ThrowIfNull(workspace);

        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        user = await _userDao.SelectWorkspaceAsync(user, workspace);
        return await _userDtoBuilder.BuildAsync(user);
    }
}
