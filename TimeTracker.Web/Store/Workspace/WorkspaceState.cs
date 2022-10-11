using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Workspace;

[FeatureState]
public record WorkspaceState
{
    public ICollection<WorkspaceDto> List { get; set; } = new List<WorkspaceDto>();
    
    public bool IsListLoading { get; set; }
    
    public bool IsLoaded { get; set; } = false;
}
