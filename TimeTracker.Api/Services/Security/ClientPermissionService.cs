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
            [WorkspacePermission.CreateMemberPayment] = AccessLevel.Read,
            [WorkspacePermission.UpdateMemberPayment] = AccessLevel.Read
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
            if (await _securityManager.HasAccess(permissionMapItem.Value, user, workspace))
            {
                permissions.Add(permissionMapItem.Key);
            }
        }

        return permissions;
    }
}
