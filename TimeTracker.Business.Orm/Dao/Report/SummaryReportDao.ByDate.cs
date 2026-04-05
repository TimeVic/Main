using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    #region By Date
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOwnerOrManagerAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByDaysReportItemDto>(
            "Report.SummaryByDayForOwner",
            workspaceId,
            startDate,
            endDate
        );
    }
    
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByDaysReportItemDto>(
            "Report.SummaryByDayForOthers",
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
    #endregion
}
