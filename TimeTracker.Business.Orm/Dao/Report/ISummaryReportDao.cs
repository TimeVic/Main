using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface ISummaryReportDao: IDomainService
{
    Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );
}
