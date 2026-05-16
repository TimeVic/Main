using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RegistrationStep2RequestHandler : IAsyncRequestHandler<RegistrationStep2Request, RegistrationStep2ResponseDto>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly IUserDao _userDao;
        private readonly IHttpCookiesService _cookiesService;

        public RegistrationStep2RequestHandler(
            IRegistrationService registrationService,
            IJwtAuthService jwtAuthService,
            IAuthorizationService authorizationService,
            IMapper mapper,
            IUserDao userDao,
            IHttpCookiesService cookiesService
        )
        {
            _registrationService = registrationService;
            _jwtAuthService = jwtAuthService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _userDao = userDao;
            _cookiesService = cookiesService;
        }
    
        public async Task<RegistrationStep2ResponseDto> ExecuteAsync(RegistrationStep2Request request)
        {
            var user = await _registrationService.ActivateUser(request.Token, request.Password);
            var defaultWorkspace = await _userDao.GetDefaultWorkspace(user);

            var loginResponse = await _authorizationService.Login(user);
            var userDto = _mapper.Map<UserDto>(loginResponse.User);
            userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
            _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
            return new RegistrationStep2ResponseDto()
            {
                User = userDto
            };
        }
    }
}
