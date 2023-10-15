using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, LoginResponseDto>
    {
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserDao _userDao;

        public LoginRequestHandler(
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserDao userDao
        )
        {
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userDao = userDao;
        }
    
        public async Task<LoginResponseDto> ExecuteAsync(LoginRequest request)
        {
            var loginResponse = await _authorizationService.Login(request.Email, request.Password);
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
}
