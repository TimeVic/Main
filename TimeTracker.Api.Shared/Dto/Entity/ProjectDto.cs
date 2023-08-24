using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class ProjectDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsBillableByDefault { get; set; }
    
    public decimal? DefaultHourlyRate { get; set; }
    
    public bool IsArchived { get; set; }
    
    public ClientDto? Client { get; set; }
    
    #region Select list methods
    
    // Note: this is important so the MudSelect can compare pizzas
    public override bool Equals(object o)
    {
        var other = o as ProjectDto;
        return other?.Id == Id;
    }

    // Note: this is important too!
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    // Implement this for the Pizza to display correctly in MudSelect
    public override string ToString() => Name;
    
    #endregion
}
