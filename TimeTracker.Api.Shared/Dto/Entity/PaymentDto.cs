using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class PaymentDto : IResponse
{
    public long Id { get; set; }
    
    public DateTime PaymentTime { get; set; }
    
    public string? Description { get; set; }
    
    public decimal Amount { get; set; }
    
    public ProjectDto? Project { get; set; }
    
    public ClientDto Client { get; set; }
}
