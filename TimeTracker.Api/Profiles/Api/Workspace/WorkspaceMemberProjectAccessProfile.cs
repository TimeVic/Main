using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceMemberProjectAccessProfile : Profile
{
    public WorkspaceMemberProjectAccessProfile()
    {
        CreateMap<WorkspaceMemberProjectAccessEntity, WorkspaceMemberProjectAccessDto>();
    }
}
