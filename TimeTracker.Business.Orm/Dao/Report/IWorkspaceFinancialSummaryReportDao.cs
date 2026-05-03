using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface IWorkspaceFinancialSummaryReportDao : IDomainService
{
    Task<ICollection<FinancialClientBalanceItemDto>> GetClientBalancesAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<FinancialMemberBalanceItemDto>> GetMemberBalancesAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<FinancialProjectProfitabilityItemDto>> GetProjectProfitabilityAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    );
}
