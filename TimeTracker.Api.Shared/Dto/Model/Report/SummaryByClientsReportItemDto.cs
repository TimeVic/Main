namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByClientsReportItemDto
{
    public Guid? ClientId { get; set; }

    public string? ClientName { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public decimal Amount { get; set; }
}
