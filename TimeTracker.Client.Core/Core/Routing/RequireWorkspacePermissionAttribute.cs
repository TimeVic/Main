using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Client.Core.Core.Routing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequireWorkspacePermissionAttribute : Attribute
{
    public RequireWorkspacePermissionAttribute(WorkspacePermission permission, string? unauthorizedUserRedirectUrl = null)
    {
        Permission = permission;
        UnauthorizedUserRedirectUrl = unauthorizedUserRedirectUrl;
    }

    public WorkspacePermission Permission { get; }

    public string? UnauthorizedUserRedirectUrl { get; }
}
