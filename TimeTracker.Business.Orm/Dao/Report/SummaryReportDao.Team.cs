using NHibernate.Transform;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports.TeamSummary;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao
{
    public async Task<ICollection<TeamSummaryByDaysReportItemDto>> GetTeamReportByDayAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetTeamReportAsync<TeamSummaryByDaysReportItemDto>(
            "Report.TeamSummary.ByDay",
            workspaceId,
            startDate,
            endDate
        );
    }

    public async Task<ICollection<TeamSummaryMemberReportItemDto>> GetTeamReportByMemberAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetTeamReportAsync<TeamSummaryMemberReportItemDto>(
            "Report.TeamSummary.ByMember",
            workspaceId,
            startDate,
            endDate
        );
    }

    private async Task<ICollection<T>> GetTeamReportAsync<T>(
        string queryPath,
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery(queryPath))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
}
