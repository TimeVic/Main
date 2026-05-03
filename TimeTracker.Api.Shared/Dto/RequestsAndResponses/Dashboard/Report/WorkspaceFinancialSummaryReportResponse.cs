using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class WorkspaceFinancialSummaryReportResponse : IResponse
{
    public bool IsTeamWorkspace { get; set; }

    public WorkspaceFinancialSummaryTotalsDto Totals { get; set; } = null!;

    public ICollection<WorkspaceFinancialClientBalanceDto> ClientBalances { get; set; } = new List<WorkspaceFinancialClientBalanceDto>();

    public ICollection<WorkspaceFinancialMemberBalanceDto> MemberBalances { get; set; } = new List<WorkspaceFinancialMemberBalanceDto>();

    public ICollection<WorkspaceFinancialProjectProfitabilityDto> ProjectProfitability { get; set; } = new List<WorkspaceFinancialProjectProfitabilityDto>();
}
