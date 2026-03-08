using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class DeleteRequest : IRequest
    {
        [Required]
        public Guid PaymentId { get; set; }
    }
}
