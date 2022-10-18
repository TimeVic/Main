using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao;
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
            var (jwtToken, user) = await _authorizationService.Login(request.Email, request.Password);
            var defaultWorkspace = await _userDao.GetDefaultWorkspace(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
            return new LoginResponseDto()
            {
                Token = jwtToken,
                User = userDto
            };
        }
    }
}
