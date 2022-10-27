using NHibernate;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public class SummaryReportDao: ISummaryReportDao
{
    private readonly IDbSessionProvider _sessionProvider;

    private const string SqlQuerySummaryByProject = @"
        select
            te.project_id,
            sum(extract(epoch from te.end_time - te.start_time)) as duration
        from time_entries te 
        group by te.project_id
    ";
    
    private const string SqlQuerySummaryByClient = @"
        select
            c.id,
            sum(extract(epoch from te.end_time - te.start_time)) as duration
        from time_entries te 
        left join projects p on p.id = te.project_id
        left join clients c on c.id = p.client_id 
        group by c.id
    ";
    
    private const string SqlQuerySummaryByUser = @"
        select
            te.user_id,
            sum(extract(epoch from te.end_time - te.start_time)) as duration
        from time_entries te 
        group by te.user_id
    ";
    
    private const string SqlQuerySummaryByMonth = @"
        select
            extract(month from te.date) as month,
            sum(extract(epoch from te.end_time - te.start_time)) as duration
        from time_entries te 
        group by month
        order by month
    ";
    
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

    public async Task<SummaryReportDto> GetSummaryReport(
        MembershipAccessType accessType,
        long workspaceId,
        SummaryReportType type,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        if (
            accessType != MembershipAccessType.Manager 
            && accessType != MembershipAccessType.Owner
            && availableProjectsForUser == null
        )
        {
            throw new ArgumentNullException(nameof(availableProjectsForUser));
        }
        var report = new SummaryReportDto
        {
            ByDays = await GetReportByDayAsync(
                accessType,
                workspaceId,
                startDate,
                endDate,
                availableProjectsForUser
            )
        };

        return report;
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
    
    private async Task<ICollection<ByDaysReportItemDto>> GetReportByDayAsync(
        MembershipAccessType accessType,
        long workspaceId,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        IQuery query;
        if (accessType is MembershipAccessType.Manager or MembershipAccessType.Owner)
        {
            query = _sessionProvider.CurrentSession.CreateSQLQuery(SqlQuerySummaryByDayForOwner)
                .SetParameter("workspaceId", workspaceId);
        }
        else
        {
            if (availableProjectsForUser == null)
            {
                throw new ArgumentNullException(nameof(availableProjectsForUser));
            }
            query = _sessionProvider.CurrentSession.CreateSQLQuery(SqlQuerySummaryByDayForOthers)
                .SetParameterList(
                    "projectIds",
                    availableProjectsForUser.Select(item => item.Id).ToArray()
                );
        }
        return await query
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<ByDaysReportItemDto>())
            .ListAsync<ByDaysReportItemDto>();
    }
    #endregion
}
