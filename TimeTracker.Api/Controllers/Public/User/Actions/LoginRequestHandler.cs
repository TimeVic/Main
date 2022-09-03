using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, LoginResponseDto>
    {
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public LoginRequestHandler(
            IMapper mapper,
            IAuthorizationService authorizationService
        )
        {
            _mapper = mapper;
            _authorizationService = authorizationService;
        }
    
        public async Task<LoginResponseDto> ExecuteAsync(LoginRequest request)
        {
            var (jwtToken, user) = await _authorizationService.Login(request.Email, request.Password);
            return new LoginResponseDto()
            {
                Token = jwtToken,
                User = _mapper.Map<UserDto>(user)
            };
        }
    }
}
