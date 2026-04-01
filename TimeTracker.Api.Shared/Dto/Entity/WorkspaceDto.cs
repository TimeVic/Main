using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceDto: BaseDto
{   
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public CurrencyDto Currency { get; set; }
    public string TimeZone { get; set; }
    
    public MembershipAccessType? CurrentUserAccess { get; set; }

    public bool IsFullAccess
    {
        get => CurrentUserAccess is MembershipAccessType.Manager or MembershipAccessType.Owner;
    }
}
