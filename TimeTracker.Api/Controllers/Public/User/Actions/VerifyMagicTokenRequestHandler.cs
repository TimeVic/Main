using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class VerifyMagicTokenRequestHandler : IAsyncRequestHandler<VerifyMagicTokenRequest, LoginResponseDto>
{
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserDao _userDao;

    public VerifyMagicTokenRequestHandler(
        IMapper mapper,
        IAuthorizationService authorizationService,
        IUserDao userDao
    )
    {
        _mapper = mapper;
        _authorizationService = authorizationService;
        _userDao = userDao;
    }

    public async Task<LoginResponseDto> ExecuteAsync(VerifyMagicTokenRequest request)
    {
        var loginResponse = await _authorizationService.LoginByMagicToken(request.Token);
        var userDto = _mapper.Map<UserDto>(loginResponse.User);
        var defaultWorkspace = await _userDao.GetDefaultWorkspace(loginResponse.User);
        userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
        return new LoginResponseDto()
        {
            JwtToken = loginResponse.JwtToken,
            AccessToken = loginResponse.AccessToken,
            User = userDto
        };
    }
}
