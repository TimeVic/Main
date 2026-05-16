using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class VerifyMagicTokenRequestHandler : IAsyncRequestHandler<VerifyMagicTokenRequest, LoginResponseDto>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpCookiesService _cookiesService;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public VerifyMagicTokenRequestHandler(
        IAuthorizationService authorizationService,
        IHttpCookiesService cookiesService,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _authorizationService = authorizationService;
        _cookiesService = cookiesService;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<LoginResponseDto> ExecuteAsync(VerifyMagicTokenRequest request)
    {
        var loginResponse = await _authorizationService.LoginByMagicToken(request.Token);
        var userDto = await _userDtoBuilder.BuildAsync(loginResponse.User);
        _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
        return new LoginResponseDto()
        {
            User = userDto
        };
    }
}
