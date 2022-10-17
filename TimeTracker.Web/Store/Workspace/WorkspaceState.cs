using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Workspace;

[FeatureState]
public record WorkspaceState
{
    public ICollection<WorkspaceDto> List { get; set; } = new List<WorkspaceDto>();
    
    public ICollection<WorkspaceDto> SortedList
    {
        get
        {
            var query = List.AsQueryable();
            if (HasItemToAdd)
            {
                query = query.OrderBy(item => item.Id);
            }
            return query.ToList();
        }
    }
    
    public bool HasItemToAdd
    {
        get => ItemToAdd != null;
    }
    
    public WorkspaceDto? ItemToAdd
    {
        get => List.FirstOrDefault(item => item.Id == 0);
    }
    
    public int TotalCount { get; set; }
    
    public bool IsListLoading { get; set; }
    
    public bool IsLoaded { get; set; } = false;
}
