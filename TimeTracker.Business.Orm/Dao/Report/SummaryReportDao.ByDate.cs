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
    #region By Date
    private const string SqlQuerySummaryByDayForOwner = @"
        select
            te.date as Date,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
            sum(
	            round(
	                te.hourly_rate / 60 / 60 -- Price per second 
	                *
	                extract(epoch from te.end_time - te.start_time), -- Total seconds
	                2
	            )
            ) as AmountOriginal
        from time_entries te
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by te.date
        order by te.date desc
        limit 60
    ";

    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByDaysReportItemDto>(
            SqlQuerySummaryByDayForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByDayForOthers = @"
        select
            te.date as Date,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
            sum(
                case when te.user_id = :userId
                    then round(
	                    te.hourly_rate / 60 / 60 -- Price per second 
	                    *
	                    extract(epoch from te.end_time - te.start_time), -- Total seconds
	                    2
	                )
                    else 0
                end
            ) as AmountOriginal
        from time_entries te
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by te.date
        order by te.date desc
        limit 60
    ";
    
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByDaysReportItemDto>(
            SqlQuerySummaryByDayForOthers,
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
    #endregion
}
