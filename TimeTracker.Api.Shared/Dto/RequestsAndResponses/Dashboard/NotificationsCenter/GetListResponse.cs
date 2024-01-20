using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter
{
    public class GetListResponse: PaginatedListDto<NotificationDto>
    {
        public GetListResponse(
            ICollection<NotificationDto> responseList,
            int totalItems
        ) : base(responseList, totalItems)
        {
        }
    }
}
