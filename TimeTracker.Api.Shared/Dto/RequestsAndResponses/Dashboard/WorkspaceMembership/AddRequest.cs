using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership
{
    public class AddRequest : IRequest<WorkspaceMembershipDto>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
