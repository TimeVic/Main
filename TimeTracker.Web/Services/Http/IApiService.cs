using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Web.Services.Http
{
    public interface IApiService
    {
        #region User
        Task<LoginResponseDto> LoginAsync(LoginRequest model);
        Task<bool> CheckIsLoggedInAsync(string token);
        #endregion

        #region Registration

        Task<bool> RegistrationStep1Async(RegistrationStep1Request model);
        Task<RegistrationStep2ResponseDto> RegistrationStep2Async(RegistrationStep2Request model);

        #endregion
    }
}
