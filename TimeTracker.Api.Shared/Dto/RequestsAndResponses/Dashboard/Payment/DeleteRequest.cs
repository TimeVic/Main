using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment
{
    public class DeleteRequest : IRequest
    {
        [Required]
        [IsPositive]
        public long PaymentId { get; set; }
    }
}
