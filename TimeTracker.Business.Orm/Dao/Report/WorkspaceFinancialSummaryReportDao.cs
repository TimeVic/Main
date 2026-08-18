using Autofac;
using NHibernate.Transform;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto.Reports;
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

    public async Task<ICollection<UserPaymentReportProjectItemDto>> GetUserPaymentReportProjectEarningsAsync(
        Guid workspaceId,
        Guid userId,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.UserPaymentReportProjectEarnings"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("userId", userId)
            .SetParameter("endDate", endDate)
            .SetResultTransformer(Transformers.AliasToBean<UserPaymentReportProjectItemDto>())
            .ListAsync<UserPaymentReportProjectItemDto>();
    }

    public async Task<ICollection<UserPaymentReportClientPaymentItemDto>> GetUserPaymentReportClientPaymentsAsync(Guid workspaceId, DateTime endDate)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.UserPaymentReportClientPayments"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("endDate", endDate)
            .SetResultTransformer(Transformers.AliasToBean<UserPaymentReportClientPaymentItemDto>())
            .ListAsync<UserPaymentReportClientPaymentItemDto>();
    }

    public async Task<ICollection<FinancialMemberBalanceItemDto>> GetMemberBalancesAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialMemberBalances"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialMemberBalanceItemDto>())
            .ListAsync<FinancialMemberBalanceItemDto>();
    }

    public async Task<ICollection<FinancialClientProjectItemDto>> GetClientProjectBreakdownAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialClientProjects"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialClientProjectItemDto>())
            .ListAsync<FinancialClientProjectItemDto>();
    }

    public async Task<ICollection<FinancialMemberProjectItemDto>> GetMemberProjectBreakdownAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialMemberProjects"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialMemberProjectItemDto>())
            .ListAsync<FinancialMemberProjectItemDto>();
    }

    public async Task<ICollection<FinancialProjectProfitabilityItemDto>> GetProjectProfitabilityAsync(Guid workspaceId)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.WorkspaceFinancialProjectProfitability"))
            .SetParameter("workspaceId", workspaceId)
            .SetResultTransformer(Transformers.AliasToBean<FinancialProjectProfitabilityItemDto>())
            .ListAsync<FinancialProjectProfitabilityItemDto>();
    }
}
