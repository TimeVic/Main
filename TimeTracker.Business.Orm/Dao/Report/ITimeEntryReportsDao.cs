using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface ITimeEntryReportsDao: IDomainService
{
    Task<ICollection<ProjectMemberPaymentsReportItemDto>> GetProjectMemberPaymentsReport(
        Guid workspaceId,
        Guid userId,
        DateTime endDate
    );
}
