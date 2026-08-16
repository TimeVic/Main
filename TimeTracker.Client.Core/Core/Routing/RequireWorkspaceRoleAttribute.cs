using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Client.Core.Core.Routing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequireWorkspaceRoleAttribute : Attribute
{
    public RequireWorkspaceRoleAttribute(params MembershipAccessType[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }

    public IReadOnlyCollection<MembershipAccessType> AllowedRoles { get; }
}
