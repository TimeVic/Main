using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment
{
    public class AddRequest : IRequest<MemberPaymentDto>
    {
        [RequiredNonEmpty]
        public Guid WorkspaceId { get; set; }
        
        [Display(Name = "Project")]
        [RequiredNonEmpty]
        public Guid ProjectId { get; set; }

        [Display(Name = "Member")]
        public Guid MemberId { get; set; }
        
        [Required]
        [Display(Name = "Payment Time")]
        public DateTime PaymentTime { get; set; }
    
        [StringLength(512)]
        public string? Description { get; set; }
    
        [Required]
        [IsPositive]
        public decimal Amount { get; set; }
    }
}
