using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        public Guid MemberPaymentId { get; set; }

        public void Fill(MemberPaymentDto payment)
        {
            MemberPaymentId = payment.Id;
            ProjectId = payment.Project.Id;
            MemberId = payment.Member.Id;
            Amount = payment.Amount;
            Description = payment.Description;
            PaymentTime = payment.PaymentTime;
        }
    }
}
