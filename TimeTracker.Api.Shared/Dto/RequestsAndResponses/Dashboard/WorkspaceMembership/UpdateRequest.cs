using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership
{
    public class UpdateRequest : IRequest<WorkspaceMembershipDto>
    {
        [Required]
        [IsPositive]
        public long MembershipId { get; set; }
        
        [Required]
        public MembershipAccessType Access { get; set; }
        
        public long[]? ProjectIds { get; set; }
    }
}
