using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialProjectProfitabilityDto
{
    public ProjectDto Project { get; set; } = null!;

    public ClientDto? Client { get; set; }

    public TimeSpan Duration { get; set; }

    public decimal ClientEarned { get; set; }

    public decimal TeamCost { get; set; }

    public decimal EstimatedMargin { get; set; }

    public decimal? MarginPercent { get; set; }
}
