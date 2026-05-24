using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Services
{
    public class AuthorizationService: IAuthorizationService
    {   
        private readonly ApiService _apiService;
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ApiService apiService, 
            IDispatcher dispatcher,
            ILogger<AuthorizationService> logger
        )
        {
            _apiService = apiService;
            _dispatcher = dispatcher;
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
                var workspace = user.SelectedWorkspace ?? user.DefaultWorkspace;
                if (workspace != null)
                {
                    _dispatcher.Dispatch(new LoginAction(user, workspace));
                }
            }
        }

        public async Task<bool> CheckIsLoggedInAsync()
        {
            try
            {
                var isLoggedIn = await _apiService.CheckIsLoggedInAsync();
                if (!isLoggedIn)
                {
                    await LogoutAsync();
                }

                return isLoggedIn;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "CheckIsLoggedIn returned: false");
                return false;
            }
        }
    }
}
