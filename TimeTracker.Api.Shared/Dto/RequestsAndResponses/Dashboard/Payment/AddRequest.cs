using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class AddRequest : IRequest<PaymentDto>
    {
        [Required]
        public Guid WorkspaceId { get; set; }
        
        [Required]
        [IsPositive(ErrorMessage = "Client is required")]
        public Guid ClientId { get; set; }
        
        [IsPositive(AllowZero = true)]
        public Guid ProjectId { get; set; }
        
        [Required]
        public DateTime PaymentTime { get; set; } = DateTime.Now;
    
        [StringLength(512)]
        public string? Description { get; set; }
    
        [Required]
        [IsPositive]
        public decimal Amount { get; set; }
    }
}
