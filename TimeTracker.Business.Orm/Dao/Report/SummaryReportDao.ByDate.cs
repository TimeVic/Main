using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    #region By Date
    private const string SqlQuerySummaryByDayForOwner = @"
        select
            cast(x.day as timestamp) as date,
            sum(extract(epoch from x.duration)) as durationasepoch,
            sum(
                round(
                    (extract(epoch from x.duration) / 3600.0) * te.hourly_rate,
                    2
                )
            ) as amountoriginal
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
        group by x.day
        order by x.day desc
        limit 60;
    ";

    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOwnerOrManagerAsync(
        Guid workspaceId,
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
            cast(x.day as timestamp) as Date,
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
        group by x.day
        order by x.day desc
        limit 60
    ";
    
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        Guid userId,
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
