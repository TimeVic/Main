using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    public async Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOwnerOrManagerAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByUsersReportItemDto>(
            "Report.SummaryByUserForOwner",
            workspaceId,
            startDate,
            endDate
        );
    }
    
    public async Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByUsersReportItemDto>(
            "Report.SummaryByUserForOther",
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
