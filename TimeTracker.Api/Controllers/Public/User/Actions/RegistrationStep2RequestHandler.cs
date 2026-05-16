using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RegistrationStep2RequestHandler : IAsyncRequestHandler<RegistrationStep2Request, RegistrationStep2ResponseDto>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpCookiesService _cookiesService;
        private readonly IUserDtoBuilder _userDtoBuilder;

        public RegistrationStep2RequestHandler(
            IRegistrationService registrationService,
            IJwtAuthService jwtAuthService,
            IAuthorizationService authorizationService,
            IHttpCookiesService cookiesService,
            IUserDtoBuilder userDtoBuilder
        )
        {
            _registrationService = registrationService;
            _jwtAuthService = jwtAuthService;
            _authorizationService = authorizationService;
            _cookiesService = cookiesService;
            _userDtoBuilder = userDtoBuilder;
        }
    
        public async Task<RegistrationStep2ResponseDto> ExecuteAsync(RegistrationStep2Request request)
        {
            var user = await _registrationService.ActivateUser(request.Token, request.Password);

            var loginResponse = await _authorizationService.Login(user);
            var userDto = await _userDtoBuilder.BuildAsync(loginResponse.User);
            _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
            return new RegistrationStep2ResponseDto()
            {
                User = userDto
            };
        }
    }
}
