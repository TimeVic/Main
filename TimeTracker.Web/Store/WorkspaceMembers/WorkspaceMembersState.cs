using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.WorkspaceMembers;

[FeatureState]
public record WorkspaceMembersState
{
    public ICollection<WorkspaceMemberDto> List { get; set; } = new List<WorkspaceMemberDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;
}
