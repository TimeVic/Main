using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Web.Core.Routing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequireWorkspacePermissionAttribute : Attribute
{
    public RequireWorkspacePermissionAttribute(WorkspacePermission permission)
    {
        Permission = permission;
    }

    public WorkspacePermission Permission { get; }
}
