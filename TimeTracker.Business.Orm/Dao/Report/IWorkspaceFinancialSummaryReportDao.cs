using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface IWorkspaceFinancialSummaryReportDao : IDomainService
{
    Task<ICollection<UserPaymentReportProjectItemDto>> GetUserPaymentReportProjectEarningsAsync(
        Guid workspaceId,
        Guid userId,
        DateTime endDate
    );

    Task<ICollection<UserPaymentReportClientPaymentItemDto>> GetUserPaymentReportClientPaymentsAsync(
        Guid workspaceId,
        DateTime endDate
    );

    Task<ICollection<FinancialClientBalanceItemDto>> GetClientBalancesAsync(Guid workspaceId);

    Task<ICollection<FinancialClientProjectItemDto>> GetClientProjectBreakdownAsync(Guid workspaceId);

    Task<ICollection<FinancialMemberBalanceItemDto>> GetMemberBalancesAsync(Guid workspaceId);

    Task<ICollection<FinancialMemberProjectItemDto>> GetMemberProjectBreakdownAsync(Guid workspaceId);

    Task<ICollection<FinancialProjectProfitabilityItemDto>> GetProjectProfitabilityAsync(Guid workspaceId);
}
