using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<LoginResponseDto?> LoginAsync(LoginRequest model)
        {
            return await PostAsync<LoginResponseDto?>(ApiUrl.Login, model);
        }

        public async Task<LoginResponseDto?> LoginAsDemoAsync()
        {
            return await GetAsync<LoginResponseDto?>(ApiUrl.LoginAsDemo);
        }

        public async Task<bool> LoginMagicAsync(LoginMagicRequest model)
        {
            try
            {
                await PostAsync<object>(ApiUrl.LoginMagic, model);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to login with magic link: {error}", e.Message);
            }
            return false;
        }

        public async Task<LoginResponseDto?> VerifyMagicTokenAsync(VerifyMagicTokenRequest model)
        {
            return await PostAsync<LoginResponseDto?>(ApiUrl.LoginMagicVerify, model);
        }
        
        public async Task<bool> CheckIsLoggedInAsync()
        {
            try
            {
                await GetAsync<object>(ApiUrl.UserCheckIsLoggedIn);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<UserDto?> UserGetCurrentAsync()
        {
            return await GetAsync<UserDto?>(ApiUrl.UserCurrent);
        }

        public async Task LogoutAsync()
        {
            await PostAsync<object>(ApiUrl.Logout);
        }

        public async Task<UserDto?> UserSelectWorkspaceAsync(Guid workspaceId)
        {
            return await PostAsync<UserDto?>(ApiUrl.UserSelectWorkspace, new SelectWorkspaceRequest
            {
                WorkspaceId = workspaceId
            });
        }

        public async Task<UserDto?> UserUpdateSettingsAsync(UpdateSettingsRequest request)
        {
            return await PostAsync<UserDto?>(ApiUrl.UserUpdateSettings, request);
        }

        public async Task UserChangePasswordAsync(ChangePasswordRequest request)
        {
            await PostAsync<object>(ApiUrl.UserChangePassword, request);
        }
        
        public async Task<bool> RegistrationStep1Async(RegistrationStep1Request model)
        {
            try
            {
                await PostAsync<object>(ApiUrl.RegistrationStep1, model);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
        
        public async Task<RegistrationStep2ResponseDto?> RegistrationStep2Async(RegistrationStep2Request model)
        {
            return await PostAsync<RegistrationStep2ResponseDto>(ApiUrl.RegistrationStep2, model);
        }
        
        public async Task<bool> ResetPasswordStep1(ResetPasswordStep1Request model)
        {
            try
            {
                await PostAsync<object>(ApiUrl.ResetPasswordStep1, model);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
        
        public async Task<bool> ResetPasswordStep2(ResetPasswordStep2Request model)
        {
            try
            {
                await PostAsync<object>(ApiUrl.ResetPasswordStep2, model);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
        
        public async Task SendNotificationToken(string token)
        {
            try
            {
                await PostAsync<object>(ApiUrl.SetNotificationToken, new SetNotificationTokenRequest()
                {
                    Token = token
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
