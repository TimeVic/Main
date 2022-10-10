using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsDefault { get; set; }
    
    public MembershipAccessType? CurrentUserAccess { get; set; }
}
