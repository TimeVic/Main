using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.WorkspaceMemberships;

[FeatureState]
public record WorkspaceMembershipsState
{
    public ICollection<WorkspaceMembershipDto> List { get; set; } = new List<WorkspaceMembershipDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;
}
