using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Services
{
    public class AuthorizationService: IAuthorizationService
    {   
        private readonly ApiService _apiService;
        private readonly IDispatcher _dispatcher;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ApiService apiService, 
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
            await Task.CompletedTask;
        }

        public async Task<bool> LoginAsync(LoginRequest model)
        {
            var loginData = await _apiService.LoginAsync(model);
            if (loginData != null)
            {
                Login(loginData.User);
                return true;
            }

            return false;
        }

        public void Login(string accessToken, string jwtToken, UserDto user)
        {
            Login(user);
        }

        public void Login(UserDto user)
        {
            if (user.Id != Guid.Empty)
            {
                user.DefaultWorkspace!.CurrentUserAccess = MembershipAccessType.Owner;
                _dispatcher.Dispatch(new LoginAction(user, user.DefaultWorkspace));
                _dispatcher.Dispatch(new PersistDataAction());
            }
        }

        public async Task<bool> IsHasJwtAsync()
        {
            var store = _serviceProvider.GetService<IState<AuthState>>();
            return await Task.FromResult(store?.Value.IsLoggedIn == true);
        }

        public async Task<bool> CheckIsLoggedInAsync()
        {
            bool isValidJwt = false;
            if (await IsHasJwtAsync())
            {
                try
                {
                    isValidJwt = await _apiService.CheckIsLoggedInAsync();
                    if (!isValidJwt)
                    {
                        await LogoutAsync();
                    }
                }
                catch (Exception)
                {
                    _logger.LogDebug(@"CheckIsLoggedIn returned: false");
                }
            }
            return isValidJwt;
        }
    }
}
