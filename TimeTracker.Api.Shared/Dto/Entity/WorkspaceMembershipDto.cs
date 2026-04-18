using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Model.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceMembershipDto: BaseDto
{   
    public MembershipAccessType Access { get; set; }
    
    public UserDto User { get; set; } = null!;

    public ICollection<WorkspaceMembershipProjectAccessDto> ProjectAccesses { get; set; } = new List<WorkspaceMembershipProjectAccessDto>();
}
