using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RefreshTokenRequestHandler : IAsyncRequestHandler<RefreshTokenRequest, RefreshTokenResponseDto>
    {
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserDao _userDao;

        public RefreshTokenRequestHandler(
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserDao userDao
        )
        {
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userDao = userDao;
        }
    
        public async Task<RefreshTokenResponseDto> ExecuteAsync(RefreshTokenRequest request)
        {
            var loginResponse = await _authorizationService.GenerateNewJwtToken(request.AccessToken, request.JwtToken);
            return new RefreshTokenResponseDto()
            {
                JwtToken = loginResponse.JwtToken,
                AccessToken = loginResponse.JwtToken
            };
        }
    }
}
