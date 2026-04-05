using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    
    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOwnerOrManagerAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByWeeksReportItemDto>(
            "Report.SummaryByWeekForOwner",
            workspaceId,
            startDate,
            endDate
        );
    }

    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByWeeksReportItemDto>(
            "Report.SummaryByWeekForOther",
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
