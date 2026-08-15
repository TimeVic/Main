using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    
    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportAsync<ByWeeksReportItemDto>(
            "Report.SummaryByWeek",
            workspaceId,
            userId,
            startDate,
            endDate
        );
    }
}
