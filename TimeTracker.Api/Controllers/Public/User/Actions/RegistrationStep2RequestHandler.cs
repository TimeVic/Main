using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RegistrationStep2RequestHandler : IAsyncRequestHandler<RegistrationStep2Request, RegistrationStep2ResponseDto>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IMapper _mapper;
        private readonly IUserDao _userDao;

        public RegistrationStep2RequestHandler(
            IRegistrationService registrationService,
            IJwtAuthService jwtAuthService,
            IMapper mapper,
            IUserDao userDao
        )
        {
            _registrationService = registrationService;
            _jwtAuthService = jwtAuthService;
            _mapper = mapper;
            _userDao = userDao;
        }
    
        public async Task<RegistrationStep2ResponseDto> ExecuteAsync(RegistrationStep2Request request)
        {
            var user = await _registrationService.ActivateUser(request.Token, request.Password);
            var defaultWorkspace = await _userDao.GetDefaultWorkspace(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.DefaultWorkspace = _mapper.Map<WorkspaceDto>(defaultWorkspace);
            return new RegistrationStep2ResponseDto()
            {
                JwtToken = _jwtAuthService.BuildJwt(user.Id),
                User = userDto
            };
        }
    }
}
