using Autofac;
using NHibernate.Transform;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

namespace TimeTracker.Business.Orm.Dao.Report;

public class WorkspaceFinancialSummaryReportDao : BaseDao, IWorkspaceFinancialSummaryReportDao
{
    public WorkspaceFinancialSummaryReportDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ICollection<FinancialClientBalanceItemDto>> GetClientBalancesAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialClientBalances"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<FinancialClientBalanceItemDto>())
            .ListAsync<FinancialClientBalanceItemDto>();
    }

    public async Task<ICollection<FinancialMemberBalanceItemDto>> GetMemberBalancesAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialMemberBalances"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<FinancialMemberBalanceItemDto>())
            .ListAsync<FinancialMemberBalanceItemDto>();
    }

    public async Task<ICollection<FinancialProjectProfitabilityItemDto>> GetProjectProfitabilityAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialProjectProfitability"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<FinancialProjectProfitabilityItemDto>())
            .ListAsync<FinancialProjectProfitabilityItemDto>();
    }
}
