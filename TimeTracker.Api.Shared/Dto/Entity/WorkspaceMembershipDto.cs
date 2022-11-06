using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceMembershipDto : IResponse
{
    public long Id { get; set; }
    
    public MembershipAccessType Access { get; set; }
    
    public UserDto User { get; set; }

    public ICollection<WorkspaceMembershipProjectAccessDto> ProjectAccesses { get; set; } = new List<WorkspaceMembershipProjectAccessDto>();
}
