using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class CheckLoginRequestHandler : IAsyncRequestHandler<CheckLoginRequest, CheckLoginResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;

    public CheckLoginRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
    }

    public async Task<CheckLoginResponse> ExecuteAsync(CheckLoginRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var exists = await _userDao.IsLoginExistsAsync(request.Login, user.Id);
        return new CheckLoginResponse
        {
            IsAvailable = !exists
        };
    }
}
