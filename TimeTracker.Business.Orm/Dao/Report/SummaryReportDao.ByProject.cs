using TimeTracker.Business.Orm.Dto.Reports.Summary;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao
{
    public async Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportAsync<ByProjectsReportItemDto>(
            "Report.Summary.ByProject",
            workspaceId,
            userId,
            startDate,
            endDate
        );
    }
}
