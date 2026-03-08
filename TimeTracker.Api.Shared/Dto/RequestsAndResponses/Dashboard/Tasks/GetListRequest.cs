using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        public Guid TaskListId { get; set; }

        public GetListFilterRequest? Filter { get; set; }
    }
}
