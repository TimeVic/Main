using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Constants;

namespace TimeTracker.Client.Core.Services.Security;

public interface ISecurityManager
{
    bool HasPermission(WorkspacePermission permission);

    ICollection<ProjectDto> GetSharedProjects(UserDto? user = null);

    ICollection<WorkspaceMemberDto> GetMembersWhichHaveAccessToProject(ProjectDto project);

    bool CanEditTimeEntry(TimeEntryDto? timeEntry);

    bool CanDeleteTimeEntry(TimeEntryDto? timeEntry);
}
