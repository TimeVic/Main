using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Client.Core.Services.Http;

public partial class ApiService
{
    public async Task<TimeEntryApprovalStatusSummaryDto?> TimeEntryApprovalGetStatusAsync()
    {
        return await PostAsync<TimeEntryApprovalStatusSummaryDto>(
            ApiUrl.TimeEntryApprovalStatus,
            new GetStatusRequest()
        );
    }

    public async Task<TimeEntryDto?> TimeEntryApprovalSubmitAsync(Guid timeEntryId)
    {
        return await PostAsync<TimeEntryDto>(
            ApiUrl.TimeEntryApprovalSubmit,
            new SubmitRequest { TimeEntryId = timeEntryId }
        );
    }

    public async Task<PaginatedListDto<TimeEntryDto>?> TimeEntryApprovalSubmitPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await PostAsync<PaginatedListDto<TimeEntryDto>>(
            ApiUrl.TimeEntryApprovalSubmitPeriod,
            new SubmitPeriodRequest { StartDate = startDate, EndDate = endDate }
        );
    }

    public async Task<PaginatedListDto<TimeEntryDto>?> TimeEntryApprovalApproveAsync(ICollection<Guid> timeEntryIds)
    {
        return await PostAsync<PaginatedListDto<TimeEntryDto>>(
            ApiUrl.TimeEntryApprovalApprove,
            new ApproveRequest { TimeEntryIds = timeEntryIds }
        );
    }

    public async Task<PaginatedListDto<TimeEntryDto>?> TimeEntryApprovalRejectAsync(ICollection<Guid> timeEntryIds, string reason)
    {
        return await PostAsync<PaginatedListDto<TimeEntryDto>>(
            ApiUrl.TimeEntryApprovalReject,
            new RejectRequest { TimeEntryIds = timeEntryIds, Reason = reason }
        );
    }

    public async Task<TimeEntryDto?> TimeEntryApprovalUnapproveAsync(Guid timeEntryId)
    {
        return await PostAsync<TimeEntryDto>(
            ApiUrl.TimeEntryApprovalUnapprove,
            new UnapproveRequest { TimeEntryId = timeEntryId }
        );
    }

    public async Task<GetSubmittersResponse?> TimeEntryApprovalGetSubmittersAsync()
    {
        return await PostAsync<GetSubmittersResponse>(
            ApiUrl.TimeEntryApprovalSubmitters,
            new GetSubmittersRequest()
        );
    }

    public async Task<GetApprovalDetailsResponse?> TimeEntryApprovalGetDetailsAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await PostAsync<GetApprovalDetailsResponse>(
            ApiUrl.TimeEntryApprovalDetails,
            new GetApprovalDetailsRequest
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            }
        );
    }
}
