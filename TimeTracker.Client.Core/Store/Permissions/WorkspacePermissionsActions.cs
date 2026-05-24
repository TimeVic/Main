using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;

namespace TimeTracker.Client.Core.Store.Permissions;

public record struct SetWorkspacePermissionsAction(GetWorkspacePermissionsResponse Response);

public record struct SetWorkspacePermissionsLoadingAction(bool IsLoading);

public record struct ClearWorkspacePermissionsAction();
