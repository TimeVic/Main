using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        [IsPositive]
        public int Page { get; set; } = 1;
    }
}
