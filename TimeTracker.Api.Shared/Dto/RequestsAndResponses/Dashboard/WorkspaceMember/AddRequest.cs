using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember
{
    public class AddRequest : IRequest<WorkspaceMemberDto>
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        public MembershipAccessType Access { get; set; } = MembershipAccessType.User;
    }
}
