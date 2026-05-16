using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class LoginRequestHandler : IAsyncRequestHandler<LoginRequest, LoginResponseDto>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpCookiesService _cookiesService;
        private readonly IUserDtoBuilder _userDtoBuilder;

        public LoginRequestHandler(
            IAuthorizationService authorizationService,
            IHttpCookiesService cookiesService,
            IUserDtoBuilder userDtoBuilder
        )
        {
            _authorizationService = authorizationService;
            _cookiesService = cookiesService;
            _userDtoBuilder = userDtoBuilder;
        }
    
        public async Task<LoginResponseDto> ExecuteAsync(LoginRequest request)
        {
            var loginResponse = await _authorizationService.Login(request.Email, request.Password);
            var userDto = await _userDtoBuilder.BuildAsync(loginResponse.User);
            _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
            return new LoginResponseDto()
            {
                User = userDto
            };
        }
    }
}
