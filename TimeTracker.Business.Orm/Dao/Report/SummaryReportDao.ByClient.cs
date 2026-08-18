using TimeTracker.Business.Orm.Dto.Reports.Summary;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao
{   
    public async Task<ICollection<ByClientsReportItemDto>> GetReportByClientAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportAsync<ByClientsReportItemDto>(
            "Report.Summary.ByClient",
            workspaceId,
            userId,
            startDate,
            endDate
        );
    }
}
