using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember
{
    public class AddRequest : IRequest<WorkspaceMemberDto>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
