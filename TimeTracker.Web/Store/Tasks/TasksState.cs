using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Store.Tasks;

[FeatureState]
public record TasksState
{
    public ICollection<TaskDto> List { get; set; } = new List<TaskDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;

    public GetListFilterRequest Filter { get; set; } = new();
}
