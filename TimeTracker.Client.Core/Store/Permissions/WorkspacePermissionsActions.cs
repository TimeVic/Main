using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Client.Core.Store.Permissions;

public record struct SetWorkspacePermissionsAction(Guid WorkspaceId, ICollection<WorkspacePermission> Permissions);

public record struct SetWorkspacePermissionsLoadingAction(bool IsLoading);

public record struct ClearWorkspacePermissionsAction();
