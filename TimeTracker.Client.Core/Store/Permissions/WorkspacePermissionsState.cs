using Fluxor;
using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Client.Core.Store.Permissions;

[FeatureState]
public record WorkspacePermissionsState
{
    public Guid? WorkspaceId { get; set; }

    public ICollection<WorkspacePermission> Permissions { get; set; } = new List<WorkspacePermission>();

    public bool IsLoading { get; set; }

    public bool IsLoaded { get; set; }
}
