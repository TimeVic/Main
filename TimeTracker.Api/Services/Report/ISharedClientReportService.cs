using Domain.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Services.Report;

public interface ISharedClientReportService : IDomainService
{
    Task<SharedClientReportEntity> GetActiveAsync(string token, bool isRequireTasks = false);

    Task<GetSharedClientReportResponse> GetReportAsync(SharedClientReportEntity report);

    Task<GetSharedClientReportTasksResponse> GetTasksAsync(SharedClientReportEntity report, Guid projectId, int page);
}
