using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses;
using TimeTracker.Business.Common.Constants;

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

        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetFilteredListResponse>
            TimeEntryGetFilteredListAsync(
                TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetFilteredListRequest model);
        Task TimeEntryDeleteAsync(long id);

        #endregion
        
        #region Project

        Task<ProjectDto> ProjectAddAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.AddRequest model);
        Task<ProjectDto> ProjectUpdateAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.UpdateRequest model);
        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListResponse> ProjectGetListAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListRequest model);

        #endregion

        #region Client

        Task<ClientDto> ClientAddAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.AddRequest model);
        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.GetListResponse> ClientGetListAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.GetListRequest model);

        #endregion
        
        #region Payment

        Task<PaymentDto> PaymentAddAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment.AddRequest model);
        Task<PaymentDto> PaymentUpdateAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment.UpdateRequest model);
        Task PaymentDeleteAsync(long paymentId);
        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment.GetListResponse> PaymentGetListAsync(TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment.GetListRequest model);

        #endregion
        
        #region Report

        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report.PaymentReportResponse> ReportsGetPaymentsReportAsync(long workspaceId);

        #endregion
        
        #region WorkspaceMembership

        Task<WorkspaceMembershipDto> WorkspaceMembershipAddAsync(long workspaceId, string email);

        Task<WorkspaceMembershipDto> WorkspaceMembershipUpdateAsync(
            long membershipId,
            MembershipAccessType access,
            ICollection<long>? projectIds
        );

        Task<TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership.GetListResponse>
            WorkspaceMembershipGetListAsync(
                TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership.GetListRequest model
            );

        Task WorkspaceMembershipDeleteAsync(long membershipId);

        #endregion
        
        #region Workspace
        
        Task<PaginatedListDto<WorkspaceDto>> WorkspaceGetListAsync();
        
        #endregion
    }
}
