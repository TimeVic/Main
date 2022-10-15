using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class AddRequest : IRequest<WorkspaceDto>
    {
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
