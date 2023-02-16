using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Services.Security;

public interface ISecurityManager
{
    ICollection<ProjectDto> GetSharedProjects(UserDto? user = null);

    ICollection<WorkspaceMembershipDto> GetMembersWhichHaveAccessToProject(ProjectDto project);
}
