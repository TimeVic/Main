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
    
    public bool HasItemToAdd
    {
        get => ItemToAdd != null;
    }
    
    public WorkspaceMembershipDto? ItemToAdd
    {
        get => List.FirstOrDefault(item => item.Id == 0);
    }
    
    public ICollection<WorkspaceMembershipDto> SortedList
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
}
