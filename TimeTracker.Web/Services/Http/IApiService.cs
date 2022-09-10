using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses;

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

        Task<TimeEntryDto> TimeEntryStartAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.StartRequest model);
        Task TimeEntryStopAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.StopRequest model);
        Task<TimeEntryDto> TimeEntrySetAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.SetRequest model);
        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetListResponse> TimeEntryGetListAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetListRequest model);
        Task TimeEntryDeleteAsync(long id);

        #endregion
        
        #region Project

        Task<ProjectDto> ProjectAddAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.AddRequest model);
        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListResponse> ProjectGetListAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListRequest model);

        #endregion
    }
}
