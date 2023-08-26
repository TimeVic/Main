using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Client;

[FeatureState]
public record ClientState
{
    public ICollection<ClientDto> List { get; set; } = new List<ClientDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;
}
