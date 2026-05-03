namespace TimeTracker.Api.Shared.Dto.Model.Report.WorkspaceFinancialSummary;

public class WorkspaceFinancialSummaryTotalsDto
{
    public decimal ClientEarned { get; set; }

    public decimal ClientReceived { get; set; }

    public decimal ClientOutstanding { get; set; }

    public decimal TeamCost { get; set; }

    public decimal MemberPaidOut { get; set; }

    public decimal MemberOutstanding { get; set; }

    public decimal EstimatedMargin { get; set; }

    public decimal RealizedMargin { get; set; }
}
