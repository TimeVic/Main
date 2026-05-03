using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;

public class DeleteRequest : IRequest
{
    [RequiredNonEmpty]
    public Guid ClientPaymentId { get; set; }
}
