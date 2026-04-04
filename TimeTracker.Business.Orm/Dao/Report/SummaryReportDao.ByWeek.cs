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
            cast(date_trunc('week', x.day) as timestamp) as WeekStartDate,
            cast(date_trunc('week', x.day) + interval '6 days' as timestamp) as WeekEndDate,
            sum(extract(epoch from x.duration)) as DurationAsEpoch,
            sum(
	            round(
	                (extract(epoch from x.duration) / 3600.0)
	                * te.hourly_rate,
	                2
	            )
            ) as AmountOriginal
        from time_entries te
        join workspaces w on w.id = te.workspace_id
        cross join lateral fn_split_time_entry_by_day(
            te.start_time,
            te.end_time,
            w.time_zone
        ) as x
        where te.workspace_id = :workspaceId
          and x.day >= cast(:startDate as date)
          and x.day <= cast(:endDate as date)
        group by WeekStartDate, WeekEndDate
        order by WeekStartDate desc
    ";
    
    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOwnerOrManagerAsync(
        Guid workspaceId,
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
            cast(date_trunc('week', x.day) as timestamp) as WeekStartDate,
            cast(date_trunc('week', x.day) + interval '6 days' as timestamp) as WeekEndDate,
            sum(extract(epoch from x.duration)) as DurationAsEpoch,
            sum(
                case when te.user_id = :userId
                    then round(
	                    (extract(epoch from x.duration) / 3600.0)
	                    * te.hourly_rate,
	                    2
	                )
                    else 0
                end
            ) as AmountOriginal
        from time_entries te
        join projects p on p.id = te.project_id
        join workspaces w on w.id = p.workspace_id
        cross join lateral fn_split_time_entry_by_day(
            te.start_time,
            te.end_time,
            w.time_zone
        ) as x
        where te.project_id in (:projectIds)
          and x.day >= cast(:startDate as date)
          and x.day <= cast(:endDate as date)
        group by WeekStartDate, WeekEndDate
        order by WeekStartDate desc
    ";

    public async Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByWeeksReportItemDto>(
            SqlQuerySummaryByWeekForOther,
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
