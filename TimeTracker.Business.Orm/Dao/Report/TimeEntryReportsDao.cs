using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports;

namespace TimeTracker.Business.Orm.Dao.Report;

public class TimeEntryReportsDao: ITimeEntryReportsDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TimeEntryReportsDao(IDbSessionProvider _sessionProvider)
    {
        this._sessionProvider = _sessionProvider;
    }
    
    private const string SqlQuery = @"
        select  
            p.id as ProjectId,
            p.name as ProjectName,
            c.id as ClientId,
            c.name as ClientName,
            extract(epoch from sum(te.end_time - te.start_time)) as TotalDurationAsEpoch,
            sum(
	            (te.hourly_rate / 60 / 60) -- Price per second 
	            *
	            extract(epoch from te.end_time - te.start_time) -- Total seconds
            ) as AmountOriginal,

            (
	            select sum(pm.amount) from payments pm
	            where pm.client_id = c.id
	                and pm.user_id = :userId
	                and pm.workspace_id = :workspaceId
	                and pm.payment_time <= :endDate
	            group by pm.client_id
            ) as PaidAmountByClientOriginal,
            (
	            select sum(pm.amount) from payments pm
	            where pm.project_id = p.id
	                and pm.user_id = :userId
	                and pm.workspace_id = :workspaceId 
	                and pm.payment_time <= :endDate
	            group by pm.client_id
            ) as PaidAmountByProjectOriginal
        from time_entries te
        left join projects p on p.id = te.project_id 
        left join clients c on c.id = p.client_id
        where te.workspace_id = :workspaceId 
            and te.user_id = :userId 
            and te.is_billable = true
            and te.date <= :endDate
        group by p.id, c.id
    ";

    public async Task<ICollection<ProjectPaymentsReportItemDto>> GetProjectPaymentsReport(
        long workspaceId,
        long userId,
        DateTime endDate
    )
    {
        return await _sessionProvider.CurrentSession.CreateSQLQuery(SqlQuery)
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("userId", userId)
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<ProjectPaymentsReportItemDto>())
            .ListAsync<ProjectPaymentsReportItemDto>();
    }
}
