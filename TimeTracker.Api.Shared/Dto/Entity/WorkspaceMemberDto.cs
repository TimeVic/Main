using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Model.WorkspaceMember;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceMemberDto: BaseDto
{   
    public MembershipAccessType Access { get; set; }
    
    public UserDto User { get; set; } = null!;

    public ICollection<WorkspaceMemberProjectAccessDto> ProjectAccesses { get; set; } = new List<WorkspaceMemberProjectAccessDto>();
}
