using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        [IsPositive]
        public long TaskListId { get; set; }

        [Required]
        [IsPositive]
        public long Page { get; set; } = 1;
    }
}
