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
    private const string SqlQuerySummaryByWeekForOwner = @"
        select
            cast(extract(week from te.date) AS int) as Week,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te 
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by Week
        order by Week desc
    ";
    
    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByWeeksReportItemDto>(
            SqlQuerySummaryByWeekForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByWeekForOther = @"
        select
            cast(extract(week from te.date) AS int) as Week,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te 
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by Week
        order by Week
    ";

    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByWeeksReportItemDto>(
            SqlQuerySummaryByWeekForOther,
            startDate,
            endDate,
            availableProjectsForUser
        );
    }
}
