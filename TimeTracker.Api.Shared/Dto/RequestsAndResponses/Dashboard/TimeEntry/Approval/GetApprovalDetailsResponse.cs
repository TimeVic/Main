using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

public class GetApprovalDetailsResponse : IResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public decimal TotalDeveloperAmount { get; set; }
    public decimal TotalClientAmount { get; set; }
    public decimal ProfitMarginPercent => TotalClientAmount > 0
        ? Math.Round(((TotalClientAmount - TotalDeveloperAmount) / TotalClientAmount) * 100, 2)
        : 0;

    public IReadOnlyList<ApprovalProjectDto> Projects { get; set; } = new List<ApprovalProjectDto>();
}
