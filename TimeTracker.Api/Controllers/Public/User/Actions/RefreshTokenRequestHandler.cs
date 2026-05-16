using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RefreshTokenRequestHandler : IAsyncRequestHandler<RefreshTokenRequest, RefreshTokenResponseDto>
    {
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserDao _userDao;
        private readonly IHttpCookiesService _cookiesService;

        public RefreshTokenRequestHandler(
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
    
        public async Task<RefreshTokenResponseDto> ExecuteAsync(RefreshTokenRequest request)
        {
            var loginResponse = await _authorizationService.GenerateNewJwtToken(request.AccessToken, request.JwtToken);
            _cookiesService.AppendAuthCookies(loginResponse.AccessToken, loginResponse.JwtToken);
            return new RefreshTokenResponseDto();
        }
    }
}
