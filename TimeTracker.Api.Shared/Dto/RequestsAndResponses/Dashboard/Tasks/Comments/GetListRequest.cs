using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [Required]
        [IsPositive]
        public long TaskId { get; set; }
        
        [Required]
        [IsPositive]
        public int Page { get; set; }
    }
}
