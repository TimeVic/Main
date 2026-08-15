using TimeTracker.Api.Shared.Constants;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Services.Security;

public class ClientPermissionService : IClientPermissionService
{
    private static readonly IReadOnlyDictionary<WorkspacePermission, AccessLevel> WorkspaceAccessMap =
        new Dictionary<WorkspacePermission, AccessLevel>
        {
            [WorkspacePermission.ReadWorkspaceSettings] = AccessLevel.Read,
            [WorkspacePermission.UpdateWorkspaceSettings] = AccessLevel.Write,
            [WorkspacePermission.ReadWorkspaceMembers] = AccessLevel.Read,
            [WorkspacePermission.UpdateWorkspaceMembers] = AccessLevel.Write,
            [WorkspacePermission.CreateProject] = AccessLevel.Write,
            [WorkspacePermission.UpdateProject] = AccessLevel.Write,
            [WorkspacePermission.CreateClient] = AccessLevel.Write,
            [WorkspacePermission.UpdateClient] = AccessLevel.Write,
            [WorkspacePermission.ReadClientPayment] = AccessLevel.Read,
            [WorkspacePermission.CreateClientPayment] = AccessLevel.Write,
            [WorkspacePermission.UpdateClientPayment] = AccessLevel.Write,
            [WorkspacePermission.ReadMemberPayment] = AccessLevel.Read,
            [WorkspacePermission.CreateMemberPayment] = AccessLevel.Write,
            [WorkspacePermission.UpdateMemberPayment] = AccessLevel.Write,
            [WorkspacePermission.ReadWorkspaceFinancialSummary] = AccessLevel.Write,
            [WorkspacePermission.CreateMemberPaymentForOtherMembers] = AccessLevel.Write,
            [WorkspacePermission.ReadUserPaymentReport] = AccessLevel.Read
        };

    private static readonly HashSet<WorkspacePermission> MemberPermissions = new()
    {
        WorkspacePermission.ReadWorkspaceMembers,
        WorkspacePermission.UpdateWorkspaceMembers,
        WorkspacePermission.ReadMemberPayment,
        WorkspacePermission.CreateMemberPayment,
        WorkspacePermission.UpdateMemberPayment,
        WorkspacePermission.CreateMemberPaymentForOtherMembers
    };

    private readonly ISecurityManager _securityManager;

    public ClientPermissionService(ISecurityManager securityManager)
    {
        _securityManager = securityManager;
    }

    public async Task<ICollection<WorkspacePermission>> GetPermissionsAsync(
        UserEntity user,
        WorkspaceEntity workspace
    )
    {
        var permissions = new List<WorkspacePermission>();
        foreach (var permissionMapItem in WorkspaceAccessMap)
        {
            if (permissionMapItem.Key == WorkspacePermission.ReadWorkspaceFinancialSummary && workspace.Mode != WorkspaceMode.Team)
            {
                continue;
            }

            if (
                permissionMapItem.Key == WorkspacePermission.ReadUserPaymentReport
                && workspace.Mode == WorkspaceMode.Team
                && workspace.Members.FirstOrDefault(member => member.User.Id == user.Id)?.Access != MembershipAccessType.User
            )
            {
                continue;
            }

            if (workspace.Mode != WorkspaceMode.Team && MemberPermissions.Contains(permissionMapItem.Key))
            {
                continue;
            }

            if (await _securityManager.HasAccess(permissionMapItem.Value, user, workspace))
            {
                permissions.Add(permissionMapItem.Key);
            }
        }

        return permissions;
    }
}
