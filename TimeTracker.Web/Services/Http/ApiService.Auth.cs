using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Web.Core.Exceptions;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<LoginResponseDto> LoginAsync(LoginRequest model)
        {
            var response = await PostAsync<LoginResponseDto>(ApiUrl.Login, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<bool> CheckIsLoggedInAsync(string token)
        {
            try
            {
                await GetAsync(ApiUrl.UserCheckIsLoggedIn, null, token);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
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
        
        public async Task<RegistrationStep2ResponseDto> RegistrationStep2Async(RegistrationStep2Request model)
        {
            var response = await PostAsync<RegistrationStep2ResponseDto>(ApiUrl.RegistrationStep2, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
