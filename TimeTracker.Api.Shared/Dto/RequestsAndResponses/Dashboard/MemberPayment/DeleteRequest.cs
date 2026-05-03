using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment
{
    public class DeleteRequest : IRequest
    {
        [RequiredNonEmpty]
        public Guid MemberPaymentId { get; set; }
    }
}
