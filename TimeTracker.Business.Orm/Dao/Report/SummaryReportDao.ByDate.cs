using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportAsync<ByDaysReportItemDto>(
            "Report.SummaryByDay",
            workspaceId,
            userId,
            startDate,
            endDate
        );
    }
}
