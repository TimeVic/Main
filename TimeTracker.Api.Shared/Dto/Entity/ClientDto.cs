using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class ClientDto : IResponse
{
    public long Id { get; set; }
    
    public string Name { get; set; }
}
