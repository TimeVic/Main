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
    private readonly IDbSessionProvider _sessionProvider;
    
    private const string SqlQuerySummaryByWeek = @"
        select
            extract(week from te.date) as week,
            sum(extract(epoch from te.end_time - te.start_time)) as duration
        from time_entries te 
        group by week
        order by week
    ";

    public SummaryReportDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    private async Task<ICollection<T>> GetReportForOwnerOrManagerAsync<T>(
        string query,
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await _sessionProvider.CurrentSession.CreateSQLQuery(query)
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
    
    public async Task<ICollection<T>> GetReportForOtherAsync<T>(
        string query,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        if (availableProjectsForUser == null)
        {
            throw new ArgumentNullException(nameof(availableProjectsForUser));
        }

        if (!availableProjectsForUser.Any())
        {
            return new List<T>();
        }
        return await _sessionProvider.CurrentSession.CreateSQLQuery(query)
            .SetParameterList(
                "projectIds",
                availableProjectsForUser.Select(item => item.Id).ToArray()
            )
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
    
    #region By Date
    private const string SqlQuerySummaryByDayForOwner = @"
        select
            te.date as Date,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
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
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by te.date
        order by te.date desc
        limit 60
    ";
    
    public async Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByDaysReportItemDto>(
            SqlQuerySummaryByDayForOthers,
            startDate,
            endDate,
            availableProjectsForUser
        );
    }
    #endregion
}
