using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Services.Security;

public class SecurityManager: ISecurityManager
{
    private readonly IState<WorkspaceMembershipsState> _workspaceMembershipsState;
    private readonly IState<AuthState> _authState;
    private readonly IState<ProjectState> _projectState;

    public SecurityManager(
        IState<WorkspaceMembershipsState> workspaceMembershipsState,
        IState<AuthState> authState,
        IState<ProjectState> projectState
    )
    {
        _workspaceMembershipsState = workspaceMembershipsState;
        _authState = authState;
        _projectState = projectState;
    }

    public ICollection<ProjectDto> GetSharedProjects(UserDto? user = null)
    {
        user ??= _authState.Value.User;

        var projectAccesses = _workspaceMembershipsState
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
            .Select(item => item.Key)
            .ToList();
    }
    
    public ICollection<WorkspaceMembershipDto> GetMembersWhichHaveAccessToProject(ProjectDto project)
    {
        return _workspaceMembershipsState.Value.List.Where(
                item => item.Access == MembershipAccessType.Manager
                    || item.Access == MembershipAccessType.Owner
                    || item.ProjectAccesses.Any(projectAccess => projectAccess.Project.Id == project.Id)
            )
            .ToList();
    }
}
