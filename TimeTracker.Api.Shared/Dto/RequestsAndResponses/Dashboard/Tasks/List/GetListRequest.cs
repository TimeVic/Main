using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        [Required]
        public Guid WorkspaceId { get; set; }

        public Guid? ProjectId { get; set; }
    }
}
