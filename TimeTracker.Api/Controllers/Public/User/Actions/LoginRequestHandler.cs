using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, LoginResponseDto>
    {
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserDao _userDao;
        private readonly IHttpCookiesService _cookiesService;

        public LoginRequestHandler(
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserDao userDao,
            IHttpCookiesService cookiesService
        )
        {
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userDao = userDao;
            _cookiesService = cookiesService;
        }
    
        public async Task<LoginResponseDto> ExecuteAsync(LoginRequest request)
        {
            var loginResponse = await _authorizationService.Login(request.Email, request.Password);
            var userDto = _mapper.Map<UserDto>(loginResponse.User);
            var defaultWorkspace = await _userDao.GetDefaultWorkspace(loginResponse.User);
            userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
            _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
            return new LoginResponseDto()
            {
                User = userDto
            };
        }
    }
}
