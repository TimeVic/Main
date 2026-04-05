using NHibernate;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    public async Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOwnerOrManagerAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByMonthsReportItemDto>(
            "Report.SummaryByMonthForOwner",
            workspaceId,
            startDate,
            endDate
        );
    }

    public async Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByMonthsReportItemDto>(
            "Report.SummaryByMonthForOther",
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
