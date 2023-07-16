using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Profiles.Workspace;

public class WorkspaceSettingsProfile : Profile
{
    public WorkspaceSettingsProfile()
    {
        CreateMap<WorkspaceSettingsRedmineEntity, WorkspaceSettingsRedmineDto>();
        CreateMap<WorkspaceSettingsClickUpEntity, WorkspaceSettingsClickUpDto>();
        CreateMap<WorkspaceSettingsJiraEntity, WorkspaceSettingsJiraDto>();
    }
}
