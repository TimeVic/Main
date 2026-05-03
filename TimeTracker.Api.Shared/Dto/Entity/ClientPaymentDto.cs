using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class ClientPaymentDto: BaseDto
{
    public DateTime PaymentTime { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public ProjectDto? Project { get; set; }

    public ClientDto Client { get; set; } = null!;
}
