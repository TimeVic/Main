using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class AddRequest : IRequest<PaymentDto>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [Required]
        [IsPositive(ErrorMessage = "Client is required")]
        public long ClientId { get; set; }
        
        [IsPositive(AllowZero = true)]
        public long ProjectId { get; set; }
        
        [Required]
        public DateTime PaymentTime { get; set; } = DateTime.Now;
    
        [StringLength(512)]
        public string? Description { get; set; }
    
        [Required]
        [IsPositive]
        public decimal Amount { get; set; }
    }
}
