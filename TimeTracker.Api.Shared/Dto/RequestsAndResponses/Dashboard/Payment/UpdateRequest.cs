using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        [IsPositive]
        public long PaymentId { get; set; }

        public void Fill(PaymentDto payment)
        {
            PaymentId = payment.Id;
            ClientId = payment.Client.Id;
            ProjectId = payment.Project?.Id;
            Amount = payment.Amount;
            Description = payment.Description;
            PaymentTime = payment.PaymentTime;
        }
    }
}
