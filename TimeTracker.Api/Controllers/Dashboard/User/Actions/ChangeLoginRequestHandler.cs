using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class ChangeLoginRequestHandler : IAsyncRequestHandler<ChangeLoginRequest, UserDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public ChangeLoginRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<UserDto> ExecuteAsync(ChangeLoginRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        user = await _userDao.ChangeLoginAsync(user, request.Login);
        return await _userDtoBuilder.BuildAsync(user);
    }
}
