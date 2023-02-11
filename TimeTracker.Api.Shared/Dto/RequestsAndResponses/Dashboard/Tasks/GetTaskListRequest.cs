using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetTaskListRequest : IRequest<GetTaskListResponse>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
    }
}
