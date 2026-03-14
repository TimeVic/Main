using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceProfile : Profile
{
    public WorkspaceProfile()
    {
        CreateMap<WorkspaceEntity, WorkspaceDto>();
    }
}
