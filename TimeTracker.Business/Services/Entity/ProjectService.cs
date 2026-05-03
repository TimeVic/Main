using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Entity;

public class ProjectService: IProjectService
{
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public ProjectService(IWorkspaceAccessService workspaceAccessService)
    {
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<decimal?> GetUsersHourlyRateForProject(UserEntity user, ProjectEntity? project)
    {
        if (project == null)
        {
            return null;
        }
        var member = _workspaceAccessService.GetMemberAsync(user, project.Workspace);
        var projectAccessItem = member?.ProjectAccesses.FirstOrDefault(
            item => item.Project.Id == project.Id
        );
        if (projectAccessItem == null || projectAccessItem.HourlyRate == null)
        {
            return project.DefaultHourlyRate;
        }
        return projectAccessItem.HourlyRate;
    }
}
