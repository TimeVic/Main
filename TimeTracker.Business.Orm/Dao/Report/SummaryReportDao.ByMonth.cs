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
    private const string SqlQuerySummaryByMonthForOwner = @"
        select
            cast(extract(month from te.date) AS int) as Month,
            cast(extract(year from te.date) AS int) as Year,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te 
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by month, year
        order by month desc
    ";
    
    public async Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByMonthsReportItemDto>(
            SqlQuerySummaryByMonthForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByMonthForOther = @"
        select
            cast(extract(month from te.date) AS int) as Month,
            cast(extract(year from te.date) AS int) as Year,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te 
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by month, year
        order by month
    ";

    public async Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByMonthsReportItemDto>(
            SqlQuerySummaryByMonthForOther,
            startDate,
            endDate,
            availableProjectsForUser
        );
    }
}
