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
    private const string SqlQuerySummaryByProjectForOwner = @"
        select
            te.project_id as ProjectId,
            p.name as ProjectName,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te
        left join projects p on te.project_id = p.id
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by te.project_id, p.name
    ";
    
    public async Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByProjectsReportItemDto>(
            SqlQuerySummaryByProjectForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByProjectForOther = @"
        select
            te.project_id as ProjectId,
            p.name as ProjectName,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch
        from time_entries te
        left join projects p on te.project_id = p.id
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by te.project_id, p.name
    ";
    
    public async Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByProjectsReportItemDto>(
            SqlQuerySummaryByProjectForOther,
            startDate,
            endDate,
            availableProjectsForUser
        );
    }
}
