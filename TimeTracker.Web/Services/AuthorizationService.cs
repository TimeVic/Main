using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common.Actions;

namespace TimeTracker.Web.Services
{
    public class AuthorizationService: IAuthorizationService
    {   
        private readonly IApiService _apiService;
        private readonly IDispatcher _dispatcher;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IApiService apiService, 
            IDispatcher dispatcher,
            IServiceProvider serviceProvider,
            ILogger<AuthorizationService> logger
        )
        {
            _apiService = apiService;
            _dispatcher = dispatcher;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task LogoutAsync()
        {
            _dispatcher.Dispatch(new LogoutAction());
            _dispatcher.Dispatch(new PersistDataAction());
            await Task.CompletedTask;
        }

        public string? GetJwt()
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            return store?.Value.Jwt?.Trim();
        }
        
        public async Task<bool> LoginAsync(LoginRequest model)
        {
            var loginData = await _apiService.LoginAsync(model);
            if (loginData != null)
            {
                Login(loginData?.Token, loginData?.User);
                return true;
            }

            return false;
        }
        
        public void Login(string jwtToken, UserDto user)
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                user.DefaultWorkspace.CurrentUserAccess = MembershipAccessType.Owner;
                _dispatcher.Dispatch(new LoginAction(jwtToken, user, user.DefaultWorkspace));
                _dispatcher.Dispatch(new PersistDataAction());
            }
        }
        
        public async Task<bool> IsHasJwtAsync()
        {
            return await Task.FromResult(!string.IsNullOrEmpty(GetJwt()));
        }
        
        public async Task<bool> CheckIsLoggedInAsync()
        {
            bool isValidJwt = false;
            if (await IsHasJwtAsync())
            {
                try
                {
                    isValidJwt = await _apiService.CheckIsLoggedInAsync(GetJwt());
                    if (!isValidJwt)
                    {
                        await LogoutAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogDebug(@"CheckIsLoggedIn returned: false");
                }
            }
            return isValidJwt;
        }
    }
}
