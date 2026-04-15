using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter
{
    public class MarkAllAsReadRequest : IRequest
    {
        [RequiredNonEmpty]
        public Guid WorkspaceId { get; set; }
    }
}
