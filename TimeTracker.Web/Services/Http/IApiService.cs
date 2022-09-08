using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
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
        
        #region TimeEntry

        Task<TimeEntryDto> TimeEntryStartAsync(StartRequest model);
        Task<TimeEntryDto> TimeEntryStopAsync(StopRequest model);
        Task<TimeEntryDto> TimeEntrySetAsync(SetRequest model);
        Task<PaginatedListDto<TimeEntryDto>> TimeEntryGetListAsync(GetListRequest model);
        Task TimeEntryDeleteAsync(long id);

        #endregion
    }
}
