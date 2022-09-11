using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Project;

[FeatureState]
public record ProjectState
{
    public ICollection<ProjectDto> List { get; set; } = new List<ProjectDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }
    
    public bool HasItemToAdd
    {
        get => ItemToAdd != null;
    }
    
    public ProjectDto ItemToAdd
    {
        get => List.FirstOrDefault(item => item.Id == 0);
    }
    
    public ICollection<ProjectDto> SortedList
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
