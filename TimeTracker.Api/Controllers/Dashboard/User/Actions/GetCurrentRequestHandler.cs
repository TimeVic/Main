using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class GetCurrentRequestHandler : IAsyncRequestHandler<GetCurrentRequest, UserDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;
    private readonly IUserDao _userDao;

    public GetCurrentRequestHandler(
        IApiRequestService apiRequestService,
        IMapper mapper,
        IUserDao userDao
    )
    {
        _apiRequestService = apiRequestService;
        _mapper = mapper;
        _userDao = userDao;
    }

    public async Task<UserDto> ExecuteAsync(GetCurrentRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var userDto = _mapper.Map<UserDto>(user);
        var defaultWorkspace = await _userDao.GetDefaultWorkspace(user);
        userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
        return userDto;
    }
}
