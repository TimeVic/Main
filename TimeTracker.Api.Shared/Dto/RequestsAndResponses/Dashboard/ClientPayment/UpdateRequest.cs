using System.ComponentModel.DataAnnotations;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;

public class UpdateRequest : AddRequest
{
    [Required]
    public Guid ClientPaymentId { get; set; }

    public void Fill(ClientPaymentDto payment)
    {
        ClientPaymentId = payment.Id;
        ClientId = payment.Client.Id;
        ProjectId = payment.Project?.Id ?? Guid.Empty;
        Amount = payment.Amount;
        Description = payment.Description;
        PaymentTime = payment.PaymentTime;
    }
}
