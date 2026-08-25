using Fluxor;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Permissions;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Core.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly IState<WorkspaceMembersState> _workspaceMembersState;
    private readonly IState<AuthState> _authState;
    private readonly IState<ProjectState> _projectState;
    private readonly IState<WorkspacePermissionsState> _workspacePermissionsState;

    public SecurityManager(
        IState<WorkspaceMembersState> workspaceMembersState,
        IState<AuthState> authState,
        IState<ProjectState> projectState,
        IState<WorkspacePermissionsState> workspacePermissionsState
    )
    {
        _workspaceMembersState = workspaceMembersState;
        _authState = authState;
        _projectState = projectState;
        _workspacePermissionsState = workspacePermissionsState;
    }

    public bool HasPermission(WorkspacePermission permission)
    {
        return _workspacePermissionsState.Value.Permissions.Contains(permission);
    }

    public ICollection<ProjectDto> GetSharedProjects(UserDto? user = null)
    {
        user ??= _authState.Value.User;
        if (_authState.Value.IsRoleAdmin)
        {
            return _projectState.Value.List;
        }

        var projectAccesses = _workspaceMembersState
            .Value
            .List
            .Where(item => item.User.Id == user.Id)
            .Select(item => item.ProjectAccesses)
            .FirstOrDefault();
        if (projectAccesses == null)
        {
            return new List<ProjectDto>();
        }

        return projectAccesses.GroupBy(item => item.Project)
            .Select(
                item => _projectState.Value.List.First(x => x.Id == item.Key.Id)
            )
            .ToList();
    }
    
    public ICollection<WorkspaceMemberDto> GetMembersWhichHaveAccessToProject(ProjectDto project)
    {
        return _workspaceMembersState.Value.List.Where(
                item => item.Access == MembershipAccessType.Manager
                    || item.Access == MembershipAccessType.Owner
                    || item.ProjectAccesses.Any(projectAccess => projectAccess.Project.Id == project.Id)
            )
            .ToList();
    }

    public bool CanEditTimeEntry(TimeEntryDto? timeEntry)
    {
        if (timeEntry == null)
        {
            return false;
        }

        var workspace = _authState.Value.Workspace;
        var user = _authState.Value.User;
        if (workspace == null || user == null)
        {
            return false;
        }

        if (workspace.Mode == WorkspaceMode.Solo)
        {
            return _authState.Value.IsRoleOwner;
        }

        if (workspace.Mode == WorkspaceMode.Team)
        {
            if (timeEntry.Status is TimeEntryStatus.Pending or TimeEntryStatus.Approved)
            {
                return false;
            }

            if (timeEntry.Status is TimeEntryStatus.Draft or TimeEntryStatus.Rejected)
            {
                return timeEntry.User == null || timeEntry.User.Id == user.Id;
            }
        }

        return _authState.Value.IsRoleAdmin || timeEntry.User == null || timeEntry.User.Id == user.Id;
    }

    public bool CanDeleteTimeEntry(TimeEntryDto? timeEntry) => CanEditTimeEntry(timeEntry);
}
