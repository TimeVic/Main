using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Models;

public sealed class ProjectClientGroup
{
    public required Guid ClientId { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyCollection<ProjectDto> Projects { get; init; }
}
