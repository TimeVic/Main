using Domain.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.Services.Report;

public interface ISummaryReportService : IDomainService
{
    Task<SummaryReportResponse> GetReportAsync(
        UserEntity currentUser,
        Guid workspaceId,
        DateTime startTime,
        DateTime endTime,
        SummaryReportType type
    );
}
