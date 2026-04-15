using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class AddRequest : IRequest<PaymentDto>
    {
        [RequiredNonEmpty]
        public Guid WorkspaceId { get; set; }
        
        [Display(Name = "Client")]
        [RequiredNonEmpty]
        public Guid ClientId { get; set; }
        
        [Display(Name = "Project")]
        public Guid ProjectId { get; set; }
        
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
