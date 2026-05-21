using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Api.Shared.Dto.Entity.Notes;

public class NoteLinkDto : IResponse
{
    public Guid Id { get; set; }

    public NoteLinkEntityType EntityType { get; set; }

    public Guid EntityId { get; set; }

    public string? DisplayName { get; set; }
}
