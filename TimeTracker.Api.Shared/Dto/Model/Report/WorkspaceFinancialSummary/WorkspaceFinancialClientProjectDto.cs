using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialClientProjectDto
{
    public ProjectDto Project { get; set; } = null!;

    public TimeSpan Duration { get; set; }

    public decimal Earned { get; set; }
}
