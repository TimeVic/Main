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

    public async Task<ICollection<FinancialClientBalanceItemDto>> GetClientBalancesAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialClientBalances"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialClientBalanceItemDto>())
            .ListAsync<FinancialClientBalanceItemDto>();
    }

    public async Task<ICollection<FinancialMemberBalanceItemDto>> GetMemberBalancesAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialMemberBalances"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialMemberBalanceItemDto>())
            .ListAsync<FinancialMemberBalanceItemDto>();
    }

    public async Task<ICollection<FinancialProjectProfitabilityItemDto>> GetProjectProfitabilityAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialProjectProfitability"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialProjectProfitabilityItemDto>())
            .ListAsync<FinancialProjectProfitabilityItemDto>();
    }
}
