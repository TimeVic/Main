using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Store.Project;

[FeatureState]
public record ProjectState
{
    public ProjectDto? Selected { get; set; }
    
    public ICollection<ProjectDto> List { get; set; } = new List<ProjectDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsSaving { get; set; }
    
    public bool IsLoaded { get; set; } = false;
}
