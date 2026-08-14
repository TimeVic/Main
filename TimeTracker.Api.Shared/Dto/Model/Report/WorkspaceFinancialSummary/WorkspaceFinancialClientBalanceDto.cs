using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialClientBalanceDto
{
    public ClientDto Client { get; set; } = null!;

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }

    public decimal Received { get; set; }

    public decimal Outstanding { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public ICollection<WorkspaceFinancialClientProjectDto> Projects { get; set; } = new List<WorkspaceFinancialClientProjectDto>();
}
